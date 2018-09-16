# == Schema Information
#
# Table name: test_results
#
#  id                      :bigint(8)        not null, primary key
#  certificate_url         :string
#  duration                :string
#  minimum_pass_percentage :integer
#  passed                  :boolean
#  percentage              :integer
#  points_available        :integer
#  points_scored           :integer
#  time_finished           :time
#  time_started            :time
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  cm_link_result_id       :integer
#  link_id                 :integer
#  referee_id              :integer          not null
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

class TestResult < ApplicationRecord
  belongs_to :referee
  has_one :link, dependent: :destroy

  after_create :update_referee_certification_status, if: :passed_changed?

  private

  def update_referee_certification_status
    cert_level = link.test.level
    certification = Certification.find(level: cert_level)

    if passed
      ref_certification = RefereeCertification.find_or_create_by!(certification: certification, referee: referee)
      datetime_key = ref_certification.received_at.blank? ? 'received_at' : 'renewed_at'

      update_certification(ref_certification, datetime_key)
    else
      ref_certification = RefereeCertification.find_by(certification: certification, referee: referee)
      datetime_key = 'revoked_at'

      update_certification(ref_certification, datetime_key) if ref_certification.present?
    end
  end

  def update_certification(ref_certification, datetime_key)
    ref_certification[datetime_key] = Time.zone.now
    ref_certification.save!
  end
end
