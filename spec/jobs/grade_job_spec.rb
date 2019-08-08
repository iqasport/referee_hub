require 'rails_helper'

RSpec.describe GradeJob, type: :job do
  let(:test_to_grade) { create :test }
  let(:ref) { create :referee }
  let(:test_timestamps) { { started_at: Time.now.utc.to_s, finished_at: Time.now.utc.to_s } }
  let(:referee_answers) { [{ question_id: 1, answer_id: 2 }] }

  subject { described_class.perform_later(test_to_grade, ref, test_timestamps, referee_answers) }

  it 'should enqueue the job with the correct params' do
    ActiveJob::Base.queue_adapter = :test

    expect { subject }.to have_enqueued_job.exactly(:once)
  end
end
