class GradeJob < ApplicationJob
  queue_as :mailers
  rescue_from ActiveJob::DeserializationError do |exception|
    retry_job wait: 5.minutes unless exception.message.include? "Couldn't find"
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
