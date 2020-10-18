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
      @potential_tests = potential_tests.where(new_language_id: user.language_id) if filter_by_language?
      return potential_tests.where(level: ['assistant', 'scorekeeper']) if certifications.blank? && test_attempts.blank?

      grouped_certs = certifications.group_by { |cert| cert.version }
      levels_to_return = {}.tap do |certs_to_return|
        grouped_certs.each do |version, certs|
          cert = determine_cert_to_return(certs, version)
          certs_to_return[version] = [cert, 'scorekeeper'] if cert
        end

        potential_versions.each do |version|
          if !certs_to_return[version] && !certifications.where(version: version, level: 'head').exists?
            certs_to_return[version] = ['assistant', 'scorekeeper']
          end
        end
      end

      @potential_tests = potential_tests.where(certification_id: find_certification_ids(levels_to_return))
      return get_tests if test_attempts.present?

      potential_tests
    end

    private

    def certifications
      @certifications ||= user.certifications
    end

    def test_attempts
      @test_attempts ||= user.test_attempts
    end

    def filter_by_level_and_version(certs, level, version)
      certs.filter { |cert| cert.level == level && cert.version == version }
    end

    def determine_cert_to_return(certs, version)
      cert = nil
      if filter_by_level_and_version(certs, 'assistant', version).blank?
        cert = 'assistant'
      elsif filter_by_level_and_version(certs, 'snitch', version).blank?
        cert = 'snitch'
      elsif filter_by_level_and_version(certs, 'head', version).blank?
        cert = 'head'
      end

      cert
    end

    def find_certification_ids(levels_to_return)
      cert_ids = levels_to_return.map do |version, level|
        ids = Certification.all.where(version: version, level: level).pluck(:id)
        if level == 'head'
          next if !has_paid(ids)
        end

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

          next if test_attempts.where(test_id: t.id).first&.in_cool_down_period?

          valid_test_ids.push(t.id)
        end
      end
    end

    def has_paid(cert_ids)
      user.certification_payments.where(certification_id: cert_ids).exists?
    end

    def filter_by_language?
      test_languages = Test.all.pluck(:new_language_id).uniq
      test_languages.include?(user.language_id)
    end

    def get_tests
      valid_test_ids = find_valid_test_ids
      potential_levels = potential_tests.pluck(:level).uniq
      test_ids = valid_test_ids

      potential_levels.each do |level|
        level_tests = potential_tests.where(level: level, id: valid_test_ids)
        recert_ids = level_tests.where(recertification: true).pluck(:id)

        if certifications.where(level: level).exists?
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
