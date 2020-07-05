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

FactoryBot.define do
  factory :certification do
    level :assistant
    display_name 'Assistant Referee Certification'

    trait :snitch do
      level :snitch
      display_name 'Snitch Referee Certification'
    end

    trait :head do
      level :head
      display_name 'Head Referee Certification'
    end

    trait :field do
      level :field
      display_name 'Field Test Certification'
    end
  end
end
