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

class TestAttempt < ApplicationRecord
  belongs_to :referee, class_name: 'User'
  belongs_to :test, optional: true
  has_many :referee_answers, dependent: :destroy

  enum test_level: {
    snitch: 0,
    assistant: 1,
    head: 2,
    scorekeeper: 3
  }

  before_save :add_next_attempt, if: :test_level_changed?

  scope :snitch, -> { where(test_level: 'snitch') }
  scope :assistant, -> { where(test_level: 'assistant') }
  scope :head, -> { where(test_level: 'head') }
  scope :scorekeeper, -> { where(test_level: 'scorekeeper') }

  ONE_DAY_WINDOW = %w[snitch assistant scorekeeper].freeze

  def add_next_attempt
    if ONE_DAY_WINDOW.include?(test_level)
      self.next_attempt_at = Time.now.utc + 24.hours
    elsif test_level == 'head'
      self.next_attempt_at = Time.now.utc + 72.hours
    end
  end

  def in_cool_down_period?
    Time.now.utc < next_attempt_at
  end

  def hours_till_next_attempt
    (((next_attempt_at - Time.now.utc) / 60) / 60).round
  end
end
