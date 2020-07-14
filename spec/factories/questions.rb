# == Schema Information
#
# Table name: questions
#
#  id               :bigint           not null, primary key
#  description      :text             not null
#  feedback         :text
#  points_available :integer          default(1), not null
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  test_id          :integer          not null
#

FactoryBot.define do
  factory :question do
    description { FFaker::Lorem.paragraph }
    points_available 1
    feedback 'MyText'
    test { create :test }
  end
end
