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

require 'rails_helper'

describe RefereeAnswer, type: :model do
  let!(:cert) { create :certification }
  let!(:test) { create :test, certification_id: cert.id }
  let(:test_attempt) { create :test_attempt, test: test }
  let(:referee_answer) { build :referee_answer, test: test, test_attempt: test_attempt }

  subject { referee_answer.save }

  it 'is a valid object' do
    expect(referee_answer).to be_valid
    expect { subject }.to_not raise_error
  end

  context '#correct?' do
    let(:question) { create :question, test: test }
    let(:answer) { create :answer, correct: true, question: question }
    let!(:referee_answer) { create :referee_answer, answer: answer, question: question, test: test, test_attempt: test_attempt }

    subject { referee_answer.correct? }

    it 'should return true' do
      expect(subject).to be_truthy
    end

    context 'when answer is not correct' do
      let(:answer) { create :answer, correct: false, question: question }

      it 'should return false' do
        expect(subject).to be_falsey
      end
    end
  end
end
