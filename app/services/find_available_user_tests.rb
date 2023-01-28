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
          certs_to_return[version].push(next_eligible_level) if next_eligible_level
          # if user hasn't passed yet the SK test for this version, add it
          if user_certifications.filter { |cert| cert.level == 'scorekeeper' && cert.version == version}.blank?
            certs_to_return[version].push('scorekeeper')
          end
        end
      end

      # HACK: including recert tests
      @potential_tests = potential_tests.where('certification_id IN (?) OR (certification_id IN (?) AND recertification = true)', find_certification_ids(levels_to_return), certification_ids_for_recertification)
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
        certs_by_version['eighteen'] = [] unless certs_by_version.key?('eighteen')
        certs_by_version['twenty'] = [] unless certs_by_version.key?('twenty')
        certs_by_version['twentytwo'] = [] unless certs_by_version.key?('twentytwo')
      end
      # map a function to get the highest certification obtained for each version
      {}.tap do |highest_certifications_mut|
        certs_by_version.each do |version, certs|
          highest_certifications_mut[version] = determine_highest_eligible_cert(certs.pluck(:level))
        end
      end
    end

    def determine_highest_eligible_cert(cert_levels)
      cert_level_to_return = nil
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

      nil
    end

    def certification_ids_for_recertification
      certs = Certification.all.where(version: 'twentytwo')
      head_certs = certs.where(level: 'head')
      return certs.where.not(level: 'head').pluck(:id) if head_certs.exists? && !has_paid(head_certs.pluck(:id))

      certs.pluck(:id)
    end

    def find_certification_ids(levels_to_return)
      cert_ids = levels_to_return.map do |version, levels|
        certs = Certification.all.where(version: version, level: levels)
        head_certs = certs.where(level: 'head')
        ids = if head_certs.exists? && !has_paid(head_certs.pluck(:id))
                certs.where.not(level: 'head').pluck(:id)
              else
                certs.pluck(:id)
              end
        ids
      end

      cert_ids.flatten
    end

    def find_valid_tests
      [].tap do |valid_tests|
        potential_tests.each do |t|
          if test_attempts.blank?
            valid_tests.push(t)
            next
          end

          # skip test if in the cooldown period from an earlier attempt
          next if test_attempts.where(test_id: t.id).order('created_at DESC').first&.in_cool_down_period?
          # skip test if attempted 6 times
          if test_attempts.where(test_id: t.id).where('created_at > ?', 1.month.ago).length >= Test::MAXIMUM_RETRIES
            next
          end
          # skip test if it's recertification and attempted already once
          next if t.recertification && test_attempts.where(test_id: t.id).length >= 1

          valid_tests.push(t)
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
      valid_tests = find_valid_tests

      # HACK: only check for certification of the previous rulebook
      # TODO: make this generic in that you can recertify for N+1 version if you hold N

      # get user certifications from last version (skipping SK)
      user_certifications_from_last_version = user_certifications.where(version: 'twenty').where.not(level: 'scorekeeper')
      user_certification_levels_from_last_version = user_certifications_from_last_version.pluck(:level)
      # get list of attempts for latest recert tests
      latest_recert_tests = valid_tests.filter { |t| Certification.all.where(version: 'twentytwo').pluck(:id).include?(t.certification_id) && t.recertification == true }
      test_attempts_of_latest_recert_tests = test_attempts.where(test_id: latest_recert_tests.pluck(:id))
      # if user was certified previously and hasn't yet taken a recert test
      if !user_certification_levels_from_last_version.blank? && test_attempts_of_latest_recert_tests.blank?
        highest_cert_from_last_version = determine_highest_eligible_cert(user_certification_levels_from_last_version)
        # only show the highest recert test
        highest_recert_tests = latest_recert_tests.filter { |t| t.level == highest_cert_from_last_version }
        # older tests
        older_available_tests = valid_tests.filter { |t| !Certification.all.where(version: 'twentytwo').pluck(:id).include?(t.certification_id) && t.recertification == false }
        # return the highest recert test and any older tests
        test_ids_to_return = (older_available_tests + highest_recert_tests).select { |t| t.id }
        return potential_tests.where(id: test_ids_to_return)
      end

      potential_tests.where(id: valid_tests.pluck(:id), recertification: false)
    end
  end
end
