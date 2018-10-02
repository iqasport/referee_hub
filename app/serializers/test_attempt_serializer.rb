# == Schema Information
#
# Table name: test_attempts
#
#  id              :bigint(8)        not null, primary key
#  next_attempt_at :datetime
#  test_level      :integer
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  referee_id      :integer
#  test_id         :integer
#

class TestAttemptSerializer
  include FastJsonapi::ObjectSerializer

  attributes :next_attempt_at, :test_level
end
