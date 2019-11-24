# == Schema Information
#
# Table name: referee_answers
#
#  id              :bigint(8)        not null, primary key
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  answer_id       :bigint(8)        not null
#  question_id     :bigint(8)        not null
#  referee_id      :bigint(8)        not null
#  test_attempt_id :bigint(8)        not null
#  test_id         :bigint(8)        not null
#
# Indexes
#
#  index_referee_answers_on_answer_id        (answer_id)
#  index_referee_answers_on_question_id      (question_id)
#  index_referee_answers_on_referee_id       (referee_id)
#  index_referee_answers_on_test_attempt_id  (test_attempt_id)
#  index_referee_answers_on_test_id          (test_id)
#

class RefereeAnswer < ApplicationRecord
  belongs_to :answer
  belongs_to :question
  belongs_to :test
  belongs_to :referee, class_name: 'User'
  belongs_to :test_attempt

  delegate :correct?, to: :answer
end
