# == Schema Information
#
# Table name: tests
#
#  id                      :bigint(8)        not null, primary key
#  description             :text             not null
#  language                :string
#  level                   :integer          default("snitch")
#  minimum_pass_percentage :integer          default(80), not null
#  name                    :string
#  negative_feedback       :text
#  positive_feedback       :text
#  time_limit              :integer          default(18), not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  certification_id        :integer
#

# This model stores the test information sent by classmarker.
# It connects to our certification model to ensure the test result gives referees the right certification.
class Test < ApplicationRecord
  has_one :link, dependent: :destroy
  belongs_to :certification

  before_save :connect_to_certification

  enum level: {
    snitch: 0,
    assistant: 1,
    head: 2
  }

  def connect_to_certification
    correct_cert = Certification.find_by level: level
    current_cert = Certification.find_by id: certification_id
    return unless correct_cert.present? && current_cert.present?

    self.certification_id = correct_cert.id if current_cert&.id != correct_cert&.id
  end
end
