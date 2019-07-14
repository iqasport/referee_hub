require 'rails_helper'
require 'json'
include ActiveJob::TestHelper

RSpec.describe Services::GradeFinishedTest do
  let!(:certification) { create :certification, :snitch }
  let(:test) { create :test }
  let(:questions) { create_list(:question, 5, test: test) }
  let(:referee) { create :referee }
  let(:started_at) { Time.now.utc }
  let(:finished_at) { Time.now.utc + 15.minutes }
  let(:referee_answers) do
    questions.map do |question|
      answer = question.answers.find_by(id: question.id * 12)
      { question_id: question.id, answer_id: answer.id }
    end
  end
  let(:expected_test_result) do
    {
      test_id: test.id,
      referee_id: referee.id,
      duration: '15',
      minimum_pass_percentage: test.minimum_pass_percentage,
      passed: true,
      percentage: 100,
      points_available: 5,
      points_scored: 5,
      test_level: test.level
    }
  end

  before do
    questions.each do |question|
      create(:answer, id: question.id * 12, question: question, correct: true)
      create_list(:answer, 3, question: question)
    end
    Timecop.freeze(Time.now.utc)
  end

  subject do
    described_class.new(
      test: test,
      referee: referee,
      started_at: started_at.to_s,
      finished_at: finished_at.to_s,
      referee_answers: referee_answers
    ).perform
  end

  it 'creates a test attempt' do
    expect { subject }.to change { referee.test_attempts.count }.by(1)
    expect(referee.test_attempts.last.test).to eq test
  end

  it 'creates referee answers' do
    expect { subject }.to change { referee.referee_answers.count }.by(5)
  end

  it 'returns a test result' do
    expect(subject).to have_attributes(expected_test_result)
  end

  it 'enqueues a result email' do
    ActiveJob::Base.queue_adapter = :test
    expect { subject }.to have_enqueued_job.on_queue('mailers')
  end

  it 'delivers the email' do
    expect { perform_enqueued_jobs { subject } }.to change { ActionMailer::Base.deliveries.size }.by(1)
  end

  it 'sends to the correct referee' do
    perform_enqueued_jobs do
      subject
    end

    sent_mail = ActionMailer::Base.deliveries.last
    expect(sent_mail.to[0]).to eq referee.email
  end
end
