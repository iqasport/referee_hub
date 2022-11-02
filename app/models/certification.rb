# == Schema Information
#
# Table name: certifications
#
#  id           :bigint           not null, primary key
#  display_name :string           default(""), not null
#  level        :integer          not null
#  version      :integer          default("eighteen")
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_certifications_on_level_and_version  (level,version) UNIQUE
#

# A fairly plain data model that associates a referee to a level of certification through referee_certifications
class Certification < ApplicationRecord
  has_many :tests, dependent: :destroy
  has_many :referee_certifications, dependent: :destroy
  has_many :referees, through: :referee_certifications
  has_many :certification_payments, dependent: :destroy

  enum level: {
    snitch: 0,
    assistant: 1,
    head: 2,
    field: 3,
    scorekeeper: 4
  }

  # refers to the year the rulebook version was released
  enum version: {
    eighteen: 0,
    twenty: 1,
    twentytwo: 2
  }
end
