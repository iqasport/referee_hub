# == Schema Information
#
# Table name: test_attempts
#
#  id              :bigint           not null, primary key
#  next_attempt_at :datetime
#  test_level      :integer
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  referee_id      :integer
#  test_id         :integer
#

FactoryBot.define do
  factory :test_attempt do
    referee { create :user }
    test_level 0
  end
end
