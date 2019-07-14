module Api
  module V1
    class TestsController < ApplicationController
      before_action :authenticate_referee!
      before_action :verify_admin, only: %i[create update destroy]
      before_action :find_test, only: %i[update show destroy start finish]
      before_action :verify_valid_test_attempt, only: :start
      skip_before_action :verify_authenticity_token

      layout false

      INVALID_TEST_ATTEMPT =
        'This test is unavailable for you to take currently, please try again in a few hours'.freeze

      def index
        @tests = Test.all

        json_string = TestSerializer.new(@tests).serialized_json

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
        json_string = TestSerializer.new(@test).serialized_json

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

        json_string = QuestionSerializer.new(questions).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @test.errors.full_messages }, status: :unprocessable_entity
      end

      def finish
        test_result = Services::GradeFinishedTest.new(
          test: @test,
          referee: current_referee,
          started_at: permitted_finish_params[:started_at],
          finished_at: permitted_finish_params[:finished_at],
          referee_answers: permitted_finish_params[:referee_answers]
        ).perform

        json_string = TestResultSerializer.new(test_result).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: 'Error grading test' }, status: :unprocessable_entity
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
          :testable_question_count
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

      def verify_valid_test_attempt
        last_test_attempt =
          current_referee
          .test_attempts
          .send(@test.level)
          .order(created_at: :desc)
          .last

        return true unless last_test_attempt&.in_cool_down_period?

        render json: { error: INVALID_TEST_ATTEMPT }, status: :unauthorized
      end
    end
  end
end
