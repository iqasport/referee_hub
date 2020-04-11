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
#  referee_id              :integer          not null
#  test_id                 :integer
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

class TestResultSerializer < BaseSerializer
  attributes :duration,
             :minimum_pass_percentage,
             :passed,
             :percentage,
             :points_available,
             :points_scored,
             :time_finished,
             :time_started,
             :test_level
end
