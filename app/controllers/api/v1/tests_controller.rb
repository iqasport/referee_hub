module Api
  module V1
    class TestsController < ApplicationController
      before_action :authenticate_user!
      before_action :verify_admin, only: %i[create update destroy import export]
      before_action :find_test, only: %i[update show destroy start finish import]
      before_action :verify_valid_cool_down, only: :start
      before_action :verify_valid_tries, only: :start
      before_action :verify_eligibility, only: :start
      skip_before_action :verify_authenticity_token

      layout false

      InvalidAnswerError = Class.new(StandardError)

      INVALID_TEST_ATTEMPT =
        'This test is unavailable for you to take currently, please try again in'.freeze

      USER_NOT_ELIGIBLE = 'You are currently not eligible to take this test'.freeze

      INVALID_TRY_COUNT = 'This test is no longer available to you due to hitting the maximum amount of tries'.freeze

      INVALID_ANSWERS = 'The answers provided do not match the test, please refresh the page and try again'.freeze

      def index
        @tests = params[:active_only] ? Test.active : Test.all
        json_string = TestSerializer.new(@tests, include: [:certification]).serialized_json
        render json: json_string, status: :ok
      end

      def create
        @test = Test.new(permitted_params)
        @test.save!
        json_string = TestSerializer.new(@test).serialized_json
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def update
        @test.update!(permitted_params)
        json_string = TestSerializer.new(@test).serialized_json
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def show
        json_string = TestSerializer.new(@test, include: [:certification]).serialized_json
        render json: json_string, status: :ok
      end

      def destroy
        json_string = TestSerializer.new(@test).serialized_json
        @test.destroy!
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def start
        questions = @test.fetch_random_questions
        json_string = QuestionSerializer.new(questions, include: %i[answers]).serialized_json
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def finish
        hashed_params = permitted_finish_params.to_h
        raise InvalidAnswerError, INVALID_ANSWERS unless valid_answers?(hashed_params[:referee_answers])

        test_timestamps = { started_at: hashed_params[:started_at], finished_at: hashed_params[:finished_at] }
        enqueued_job = GradeJob.perform_later(
          @test,
          current_user,
          test_timestamps,
          hashed_params[:referee_answers]
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: "Error grading test: #{e}" }, status: :unprocessable_entity
      end

      def import
        imported_ids = Services::TestCsvImport.new(
          params['file'].tempfile.path,
          @test,
          params['mapped_headers']
        ).perform

        new_questions = @test.questions.where(id: imported_ids)
        page = params[:page] || 1
        questions_total = new_questions.count
        new_questions = new_questions.page(1)

        json_string = QuestionSerializer.new(new_questions, meta: { page: page, total: questions_total }).serialized_json
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: e.full_message }, status: :unprocessable_entity
      end

      def export
        export_options = { test_id: params[:id] }
        enqueued_job = ExportCsvJob.perform_later(
          current_user,
          'ExportedCsv::TestExport',
          export_options.to_json
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: "Error exporting test: #{e}" }, status: :unprocessable_entity
      end

      private

      def permitted_params
        params.permit(
          :description,
          :language,
          :level,
          :minimum_pass_percentage,
          :name,
          :negative_feedback,
          :positive_feedback,
          :time_limit,
          :active,
          :testable_question_count,
          :certification_id,
          :recertification,
          :new_language_id
        )
      end

      def permitted_finish_params
        params.permit(
          :started_at,
          :finished_at,
          referee_answers: %i[question_id answer_id]
        )
      end

      def find_test
        @test = Test.find_by(id: params[:id])
      end

      def verify_eligibility
        eligible_tests = current_user.available_tests
        
        # check if user is eligible to take the current test
        return true unless eligible_tests.pluck(:id).filter { |test_id| test_id == @test.id }.blank?
        
        render json: { error: USER_NOT_ELIGIBLE }, status: :unauthorized
      end

      def referee_test_attempts
        @referee_test_attempts ||= current_user.test_attempts.where(test_id: @test.id).order(created_at: :desc)
      end

      def verify_valid_cool_down
        last_test_attempt = referee_test_attempts.first

        return true unless last_test_attempt&.in_cool_down_period?

        full_message = "#{INVALID_TEST_ATTEMPT} #{last_test_attempt.hours_till_next_attempt} hours"
        render json: { error: full_message }, status: :unauthorized
      end

      def verify_valid_tries
        try_count = referee_test_attempts.where('created_at > ?', 1.month.ago).count

        return true unless try_count >= Test::MAXIMUM_RETRIES

        render json: { error: INVALID_TRY_COUNT }, status: :unauthorized
      end

      def valid_answers?(referee_answers)
        return false if referee_answers.blank?

        question_id = referee_answers[0]['question_id']

        @test.questions.where(id: question_id).exists?
      end
    end
  end
end
