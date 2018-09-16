# == Schema Information
#
# Table name: tests
#
#  id               :bigint(8)        not null, primary key
#  level            :integer          default("snitch")
#  name             :string
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  certification_id :integer
#  cm_test_id       :integer
#  link_id          :integer
#
# Indexes
#
#  index_tests_on_cm_test_id  (cm_test_id) UNIQUE
#

class Test < ApplicationRecord
  has_one :link, dependent: :destroy
  belongs_to :certifications

  before_save :connect_to_certification, if: :level_changed?

  enum level: {
    snitch: 0,
    assistant: 1,
    head: 2
  }

  def connect_to_certification
    connected_cert = Certification.find_by level: level

    self.certification_id = connected_cert.id if connected_cert.present?
  end
end
