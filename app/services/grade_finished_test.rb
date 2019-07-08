module Services
  class GradeFinishedTest
    attr_reader :test, :referee, :started_at, :referee_answers, :test_attempt, :finished_at

    def initialize(test:, referee:, started_at:, finished_at:, referee_answers: [])
      @test = test
      @referee = referee
      @started_at = Time.parse.utc(started_at)
      @finished_at = Time.parse.utc(finished_at)
      @referee_answers = referee_answers
    end

    def perform
      create_test_attempt
      create_referee_answers
      test_result = grade_answers
      send_result_email(test_result)
      test_result
    end

    private

    def create_test_attempt
      @test_attempt = TestAttempt.create!(test: test, referee: referee, test_level: test.level)
    end

    def create_referee_answers
      base_attrs = { test: test, referee: referee, test_attempt: test_attempt }

      referee_answers.each do |referee_answer|
        question_id = referee_answer[:question_id]
        next if question_id.blank?
        answer_id = referee_answer[:answer_id]
        next if answer_id.blank?

        referee_answer_attrs = base_attrs.merge(question_id: question_id, answer_id: answer_id)
        RefereeAnswer.create!(referee_answer_attrs)
      end
    end

    def grade_answers
      total_points_scored = 0
      total_points_available = 0
      test_attempt.referee_answers.includes(:question, :answer).each do |referee_answer|
        total_points_available += referee_answer.question.points_available
        total_points_scored += 1 if referee_answer.correct?
      end

      create_test_result(total_points_scored, total_points_available)
    end

    def create_test_result(points_scored, points_available)
      percentage = ((points_scored.to_f / points_available) * 100).round
      duration = ((finished_at - started_at) / 1.minute).to_i
      test_results_hash = {
        duration: duration.to_s,
        minimum_pass_percentage: test.minimum_pass_percentage,
        passed: percentage >= test.minimum_pass_percentage && duration <= test.time_limit,
        percentage: percentage,
        points_available: points_available,
        points_scored: points_scored,
        test_level: test.level,
        time_finished: finished_at,
        time_started: started_at
      }

      TestResult.create!(test_results_hash.merge(test: test, referee: referee))
    end

    def send_result_email(_test_result)
      # mailer not implemented yet
      true
    end
  end
end
