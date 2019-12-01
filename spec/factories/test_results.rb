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
#  test_level              :integer          default("snitch")
#  time_finished           :time
#  time_started            :time
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  cm_link_result_id       :integer
#  referee_id              :integer          not null
#  test_id                 :integer
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

FactoryBot.define do
  factory :test_result do
    referee { create :user }
    time_started { Time.zone.now }
    time_finished { Time.zone.now + 1.hour }
    percentage 70
    points_scored 2
    points_available 3
    passed true
    minimum_pass_percentage 70

    trait :failed do
      passed false
      percentage 30
      points_scored 1
      points_available 3
      minimum_pass_percentage 70
    end
  end
end
