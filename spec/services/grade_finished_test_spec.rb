require 'rails_helper'
require 'json'

describe Services::GradeFinishedTest do
  include ActiveJob::TestHelper

  let!(:certification) { create :certification, :snitch }
  let(:test) { create :test }
  let(:questions) { create_list(:question, 5, test: test) }
  let(:referee) { create :user }
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
      duration: '00:15:00',
      minimum_pass_percentage: test.minimum_pass_percentage,
      passed: true,
      percentage: 100,
      points_available: 5,
      points_scored: 5,
      test_level: test.level
    }
  end
  let(:skip_email) { true }

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
      referee_answers: referee_answers,
      skip_email: skip_email
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

  context 'when sending an email' do
    let(:skip_email) { false }

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

  context 'when the grade is being retried' do
    let!(:test_attempt) { create :test_attempt, referee: referee, test: test, test_level: test.level }
    let!(:test_result) { create :test_result, referee: referee, test: test, test_level: test.level }

    it 'does not create a new test attempt' do
      expect { subject }.to_not change { referee.test_attempts.count }
    end

    it 'assigns the answers to the already existing attempt' do
      expect { subject }.to change { referee.referee_answers.count }.by(5)
      expect(referee.referee_answers.last.test_attempt_id).to eq test_attempt.id
    end

    it { expect(subject).to be_nil }

    it 'does not create a new test result' do
      expect { subject }.to_not change { referee.test_results.count }
    end

    context 'when skip email is false but test result is nil' do
      let(:skip_email) { false }

      it 'does not send an email' do
        ActiveJob::Base.queue_adapter = :test

        expect { subject }.to_not have_enqueued_job
      end
    end
  end

  context 'when points scored or available are 0' do
    let(:referee_answers) { [] }
    let(:expected_test_result) do
      {
        test_id: test.id,
        referee_id: referee.id,
        duration: '00:15:00',
        minimum_pass_percentage: test.minimum_pass_percentage,
        passed: false,
        percentage: 0,
        points_available: 5,
        points_scored: 0,
        test_level: test.level
      }
    end

    it 'creates a test attempt' do
      expect { subject }.to change { referee.test_attempts.count }.by(1)
      expect(referee.test_attempts.last.test).to eq test
    end

    it 'returns a test result' do
      expect(subject).to have_attributes(expected_test_result)
    end
  end

  context 'when the test is a recertification and the referee fails' do
    let(:test) { create :test, recertification: true }
    let(:referee_answers) do
      questions.map do |question|
        answer = question.answers.where(correct: false).first
        { question_id: question.id, answer_id: answer.id }
      end
    end
    let(:skip_email) { false }

    it 'enqueues a result email' do
      ActiveJob::Base.queue_adapter = :test
      expect { subject }.to have_enqueued_job.on_queue('mailers').exactly(:twice)
    end

    it 'delivers the email' do
      expect { perform_enqueued_jobs { subject } }.to change { ActionMailer::Base.deliveries.size }.by(2)
    end

    it 'sends to the correct referee' do
      perform_enqueued_jobs do
        subject
      end

      sent_mail = ActionMailer::Base.deliveries.first
      expect(sent_mail.to[0]).to eq referee.email
    end

    it 'sends the failure email to gameplay' do
      perform_enqueued_jobs do
        subject
      end

      sent_mail = ActionMailer::Base.deliveries.last
      expect(sent_mail.to[0]).to eq 'gameplay@iqasport.org'
    end
  end
end
