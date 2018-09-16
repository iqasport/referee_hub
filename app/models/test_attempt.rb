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

class TestAttempt < ApplicationRecord
  belongs_to :test
  belongs_to :referee

  enum test_level: {
    snitch: 0,
    assistant: 1,
    head: 2
  }

  before_save :add_next_attempt, if: :test_level_changed?

  def add_next_attempt
    if test_level == 'snitch' || test_level == 'assistant'
      self.next_attempt_at = Time.now.utc + 24.hours
    elsif test_level == 'head'
      self.next_attempt_at = Time.now.utc + 72.hours
    end
  end
end
