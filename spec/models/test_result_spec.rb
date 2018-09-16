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

require 'rails_helper'

RSpec.describe TestResult, type: :model do
  pending "add some examples to (or delete) #{__FILE__}"
end
