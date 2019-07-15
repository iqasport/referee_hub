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

require 'rails_helper'

describe RefereeAnswer, type: :model do
  let(:test) { create :test }
  let(:test_attempt) { create :test_attempt, test: test }
  let(:referee_answer) { build :referee_answer, test: test, test_attempt: test_attempt }

  subject { referee_answer.save }

  it 'is a valid object' do
    expect(referee_answer).to be_valid
    expect { subject }.to_not raise_error
  end

  context '#correct?' do
    let(:answer) { create :answer, correct: true }
    let!(:referee_answer) { create :referee_answer, answer: answer, test: test, test_attempt: test_attempt }

    subject { referee_answer.correct? }

    it 'should return true' do
      expect(subject).to be_truthy
    end

    context 'when answer is not correct' do
      let(:answer) { create :answer, correct: false }

      it 'should return false' do
        expect(subject).to be_falsey
      end
    end
  end
end
