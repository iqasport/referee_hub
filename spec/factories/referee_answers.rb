# == Schema Information
#
# Table name: referee_answers
#
#  id              :bigint           not null, primary key
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  answer_id       :bigint           not null
#  question_id     :bigint           not null
#  referee_id      :bigint           not null
#  test_attempt_id :bigint           not null
#  test_id         :bigint           not null
#
# Indexes
#
#  index_referee_answers_on_answer_id        (answer_id)
#  index_referee_answers_on_question_id      (question_id)
#  index_referee_answers_on_referee_id       (referee_id)
#  index_referee_answers_on_test_attempt_id  (test_attempt_id)
#  index_referee_answers_on_test_id          (test_id)
#

FactoryBot.define do
  factory :referee_answer do
    test { create :test }
    question { create :question, test: test }
    answer { create :answer, question: question }
    referee { create :user }
    test_attempt { create :test_attempt }
  end
end
