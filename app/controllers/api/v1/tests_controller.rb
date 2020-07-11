module Api
  module V1
    class TestsController < ApplicationController
      before_action :authenticate_user!
      before_action :verify_admin, only: %i[create update destroy import]
      before_action :find_test, only: %i[update show destroy start finish import]
      before_action :verify_valid_cool_down, only: :start
      before_action :verify_valid_tries, only: :start
      skip_before_action :verify_authenticity_token

      layout false

      INVALID_TEST_ATTEMPT =
        'This test is unavailable for you to take currently, please try again in'.freeze

      INVALID_TRY_COUNT = 'This test is no longer available to you due to hitting the maximum amount of tries'.freeze

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
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def update
        @test.update!(permitted_params)
        json_string = TestSerializer.new(@test).serialized_json
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
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
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def start
        questions = @test.fetch_random_questions
        json_string = QuestionSerializer.new(questions, include: %i[answers]).serialized_json
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def finish
        hashed_params = permitted_finish_params.to_h
        test_timestamps = { started_at: hashed_params[:started_at], finished_at: hashed_params[:finished_at] }
        enqueued_job = GradeJob.perform_later(
          @test,
          current_user,
          test_timestamps,
          hashed_params[:referee_answers]
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: 'Error grading test' }, status: :unprocessable_entity
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

        json_string = QuestionSerializer.new(new_questions, meta: { page: page, total: questions_total}).serialized_json
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: exception.full_message }, status: :unprocessable_entity
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
          :certification_id
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

      def referee_test_attempts
        @referee_test_attempts ||= current_user.test_attempts.send(@test.level).order(created_at: :desc)
      end

      def verify_valid_cool_down
        last_test_attempt = referee_test_attempts.last

        return true unless last_test_attempt&.in_cool_down_period?

        full_message = "#{INVALID_TEST_ATTEMPT} #{last_test_attempt.hours_till_next_attempt} hours"
        render json: { error: full_message }, status: :unauthorized
      end

      def verify_valid_tries
        try_count = referee_test_attempts.where('created_at > ?', 1.month.ago).count

        return true unless try_count >= Test::MAXIMUM_RETRIES

        render json: { error: INVALID_TRY_COUNT }, status: :unauthorized
      end
    end
  end
end
