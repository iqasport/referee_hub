# == Schema Information
#
# Table name: tests
#
#  id               :bigint(8)        not null, primary key
#  level            :integer          default(0)
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
end
