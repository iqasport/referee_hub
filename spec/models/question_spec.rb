# == Schema Information
#
# Table name: questions
#
#  id               :bigint(8)        not null, primary key
#  description      :text             not null
#  feedback         :text
#  points_available :integer          default(1), not null
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  test_id          :integer          not null
#

require 'rails_helper'

RSpec.describe Question, type: :model do
  pending "add some examples to (or delete) #{__FILE__}"
end
