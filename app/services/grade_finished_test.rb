require 'time'

module Services
  class GradeFinishedTest
    attr_reader :test, :referee, :started_at, :referee_answers, :test_attempt, :finished_at, :skip_email

    def initialize(test:, referee:, started_at: '', finished_at: '', skip_email: false, referee_answers: [])
      @test = test
      @referee = referee
      @started_at = Time.find_zone('UTC').parse(started_at)
      @finished_at = Time.find_zone('UTC').parse(finished_at)
      @referee_answers = referee_answers
      @skip_email = skip_email
    end

    def perform
      find_or_create_test_attempt
      create_referee_answers
      test_result = grade_answers
      send_result_email(test_result) unless skip_email || test_result.blank?
      test_result
    end

    private

    def find_or_create_test_attempt
      recent_test_attempts = referee.test_attempts.send(test.level).order(created_at: :desc)
      @test_attempt = if recent_test_attempts.last&.in_cool_down_period?
                        recent_test_attempts.last
                      else
                        TestAttempt.create!(test: test, referee: referee, test_level: test.level)
                      end
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
      return nil if already_has_test_result?
      percentage = ((points_scored.to_f / points_available) * 100).round
      is_over_limit = TimeDifference.between(started_at, finished_at).in_minutes > test.time_limit
      test_results_hash = {
        duration: calculate_duration.to_s,
        minimum_pass_percentage: test.minimum_pass_percentage,
        passed: percentage >= test.minimum_pass_percentage && !is_over_limit,
        percentage: percentage,
        points_available: points_available,
        points_scored: points_scored,
        time_finished: finished_at,
        time_started: started_at
      }

      TestResult.create!(test_results_hash.merge(test: test, referee: referee, test_level: test.level))
    end

    def already_has_test_result?
      recent_test_results = referee.test_results.send(test.level).order(created_at: :desc).last
      return false if recent_test_results.blank?

      recent_test_results.created_at.to_date == Date.today
    end

    def calculate_duration
      general = TimeDifference.between(started_at, finished_at).in_general
      "00:#{general[:minutes]}:#{general[:seconds].zero? ? '00' : general[:seconds]}"
    end

    def send_result_email(test_result)
      RefereeMailer
        .with(referee: referee, test_attempt: test_attempt, test_result: test_result)
        .referee_answer_feedback_email
        .deliver_later
    end
  end
end
