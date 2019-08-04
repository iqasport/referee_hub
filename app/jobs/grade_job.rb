class GradeJob < ApplicationJob
  queue_as :default
  rescue_from ActiveJob::DeserializationError do
    retry_job wait: 5.minutes
  end

  def perform(test, referee, test_timestamps, referee_answers)
    Services::GradeFinishedTest.new(
      test: test,
      referee: referee,
      started_at: test_timestamps[:started_at],
      finished_at: test_timestamps[:finished_at],
      referee_answers: referee_answers
    ).perform
  end
end
