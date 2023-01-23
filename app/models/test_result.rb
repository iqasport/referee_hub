# == Schema Information
#
# Table name: test_results
#
#  id                      :bigint           not null, primary key
#  certificate_url         :string
#  duration                :string
#  minimum_pass_percentage :integer
#  passed                  :boolean
#  percentage              :integer
#  points_available        :integer
#  points_scored           :integer
#  test_level              :integer          default("snitch")
#  time_finished           :time
#  time_started            :time
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  referee_id              :integer          not null
#  test_id                 :integer
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

class TestResult < ApplicationRecord
  belongs_to :referee, class_name: 'User'
  belongs_to :test, optional: true

  after_create :update_referee_certification_status

  enum test_level: {
    snitch: 0,
    assistant: 1,
    head: 2,
    scorekeeper: 3
  }

  private

  def update_referee_certification_status
    if passed
      if test.recertification
        levels = []
        if test.level == 'assistant' || test.level == 'snitch' || test.level == 'head'
          certification = Certification.all.where(level: 'assistant', version: test.certification.version).first
          ref_certification = RefereeCertification.find_or_create_by!(certification: certification, referee: referee)
          datetime_key = ref_certification.received_at.blank? ? 'received_at' : 'renewed_at'

          update_certification(ref_certification, datetime_key)
        end
        if test.level == 'snitch' || test.level == 'head'
          certification = Certification.all.where(level: 'snitch', version: test.certification.version).first
          ref_certification = RefereeCertification.find_or_create_by!(certification: certification, referee: referee)
          datetime_key = ref_certification.received_at.blank? ? 'received_at' : 'renewed_at'

          update_certification(ref_certification, datetime_key)

        end
        if test.level == 'head'
          certification = Certification.all.where(level: 'head', version: test.certification.version).first
          ref_certification = RefereeCertification.find_or_create_by!(certification: certification, referee: referee)
          datetime_key = ref_certification.received_at.blank? ? 'received_at' : 'renewed_at'

          update_certification(ref_certification, datetime_key)

        end
      else
        ref_certification = RefereeCertification.find_or_create_by!(certification: test.certification, referee: referee)
        datetime_key = ref_certification.received_at.blank? ? 'received_at' : 'renewed_at'

        update_certification(ref_certification, datetime_key)
      end
    else
      ref_certification = RefereeCertification.find_by(certification: test.certification, referee: referee)
      datetime_key = 'revoked_at'

      update_certification(ref_certification, datetime_key) if ref_certification.present?
    end
  end

  def update_certification(ref_certification, datetime_key)
    ref_certification[datetime_key] = Time.zone.now
    ref_certification.needs_renewal_at = determine_needs_renewal(datetime_key)

    ref_certification.save!
  end

  def determine_needs_renewal(datetime_key)
    return Time.zone.now if datetime_key == 'revoked_at'

    nil
  end
end
