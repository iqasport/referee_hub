module Services
  class FindAvailableUserTests
    attr_reader :user, :potential_versions
    attr_accessor :potential_tests

    def initialize(user)
      @user = user
      @potential_versions = Certification.versions.keys
      @potential_tests = Test.active.all
    end

    def perform
      # filter active tests by the language
      @potential_tests = potential_tests.where(new_language_id: user.language_id) if filter_by_language?
      # fast path - if user has no record present them with any AR and SK tests
      if user_certifications.blank? && test_attempts.blank?
        return potential_tests.where(level: %w[assistant scorekeeper], recertification: false)
      end

      levels_to_return = {}.tap do |certs_to_return|
        highest_certification_per_version.each do |version, highest_user_certification|
          next_eligible_level = next_certification_level(highest_user_certification)
          certs_to_return[version] = []
          if next_eligible_level
            certs_to_return[version].push(next_eligible_level)
          end
          # if user hasn't passed yet the SK test for this version, add it
          if user_certifications.filter { |cert| cert.level == 'scorekeeper' && cert.version == version}.blank?
            certs_to_return[version].push('scorekeeper')
          end
        end
      end

      @potential_tests = potential_tests.where(certification_id: find_certification_ids(levels_to_return))
      return get_tests if test_attempts.present?

      potential_tests.where(recertification: false)
    end

    private

    def user_certifications
      @user_certifications ||= user.certifications
    end

    def test_attempts
      @test_attempts ||= user.test_attempts
    end

    def highest_certification_per_version
      # first get certifications of the user to pick the highest per version
      user_certs_by_version = user_certifications.group_by { |cert| cert.version }
      # then add missing versions with empty lists
      certs_by_version = user_certs_by_version.tap do |certs_by_version|
        certs_by_version["eighteen"] = [] if !certs_by_version.key?("eighteen")
        certs_by_version["twenty"] = [] if !certs_by_version.key?("twenty")
        certs_by_version["twentytwo"] = [] if !certs_by_version.key?("twentytwo")
      end
      # map a function to get the highest certification obtained for each version
      highest_certifications = {}.tap do |highest_certifications_mut|
        certs_by_version.each do |version, certs|
          highest_certifications_mut[version] = determine_highest_eligible_cert_to_return(certs)
        end
      end
      highest_certifications
    end

    def determine_highest_eligible_cert_to_return(certs)
      cert_level_to_return = nil
      cert_levels = certs.pluck(:level)
      if cert_levels.include?('head')
        cert = 'head'
      elsif cert_levels.include?('snitch')
        cert = 'snitch'
      elsif cert_levels.include?('assistant')
        cert = 'assistant'
      end

      cert
    end

    def next_certification_level(level)
      return 'assistant' if level.nil?
      return 'snitch' if level == 'assistant'
      return 'head' if level == 'snitch'
      return nil
    end

    def find_certification_ids(levels_to_return)
      cert_ids = levels_to_return.map do |version, levels|
        ids = Certification.all.where(version: version, level: levels).pluck(:id)
        #TODO if condifition is not met, remove Head, not just skip, because it also skips scorekeeper potentially
        next if levels.include?('head') && !has_paid(ids)

        ids
      end

      cert_ids.flatten
    end

    def find_valid_test_ids
      [].tap do |valid_test_ids|
        potential_tests.each do |t|
          if test_attempts.blank?
            valid_test_ids.push(t.id)
            next
          end

          next if test_attempts.where(test_id: t.id).order('created_at DESC').first&.in_cool_down_period?
          next if test_attempts.where(test_id: t.id).length >= 6

          valid_test_ids.push(t.id)
        end
      end
    end

    def has_paid(cert_ids)
      user.certification_payments.where(certification_id: cert_ids).exists?
    end

    def filter_by_language?
      test_languages = Test.all.pluck(:new_language_id).uniq.compact
      test_languages.include?(user.language_id)
    end

    def get_tests
      valid_test_ids = find_valid_test_ids
      potential_levels = potential_tests.pluck(:level).uniq
      test_ids = valid_test_ids

      potential_levels.each do |level|
        level_tests = potential_tests.where(level: level, id: valid_test_ids)
        recert_ids = level_tests.where(recertification: true).pluck(:id)

        # HACK: only check for certification of the previous rulebook
        # TODO: make this generic in that you can recertify for N+1 version if you hold N
        if user_certifications.where(level: level, version: 'twenty').exists?
          ids_to_remove = level_tests.pluck(:id).difference(recert_ids)
          test_ids.reject! { |id| ids_to_remove.include?(id) }
        else
          test_ids.reject! { |id| recert_ids.include?(id) }
        end
      end

      potential_tests.where(id: test_ids.flatten.uniq)
    end
  end
end
