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

FactoryBot.define do
  factory :test_result do
    cm_link_result_id 1
    user_id 1
    time_started "2018-09-16 12:57:50"
    time_finished "2018-09-16 12:57:50"
    duration "MyString"
    percentage 1
    points_scored 1
    points_available 1
    passed false
    certificate_url "MyString"
    minimum_pass_percentage 1
  end
end
