# == Schema Information
#
# Table name: certifications
#
#  id           :bigint(8)        not null, primary key
#  display_name :string           default(""), not null
#  level        :integer          not null
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_certifications_on_level  (level) UNIQUE
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
