module Services
  class FindAvailableUserTests
    attr_reader :user, :potential_versions

    def initialize(user)
      @user = user
      @potential_versions = Certification.versions.keys
    end

    def perform
      return Test.active.where(level: 'assistant') if certifications.blank?

      grouped_certs = certifications.group_by { |cert| cert.version }
      levels_to_return = {}.tap do |certs_to_return|
        grouped_certs.each do |version, certs|
          cert = determine_cert_to_return(certs, version)
          certs_to_return[version] = cert if cert
        end

        potential_versions.each do |version|
          if !certs_to_return[version]
            certs_to_return[version] = 'assistant'
          end
        end
      end

      potential_tests = Test.active.where(certification_id: find_certification_ids(levels_to_return))
      if test_attempts.present?
        return potential_tests.where(id: find_valid_test_ids(potential_tests))
      end

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
        Certification.all.where(version: version, level: level).pluck(:id)
      end

      cert_ids.flatten
    end

    def find_valid_test_ids(potential_tests)
      valid_test_attempts = test_attempts.where(test_id: potential_tests.pluck(:id)).filter do |test_attempt|
        test_attempt.next_attempt_at < DateTime.now
      end

      valid_test_attempts.map(&:test_id)
    end
  end
end
