module Api
  module V1
    class TestsController < ApplicationController
      before_action :authenticate_referee!
      before_action :verify_admin, only: %i[create update destroy]
      before_action :find_test, only: %i[update show destroy]
      skip_before_action :verify_authenticity_token

      layout false

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

      def find_test
        @test = Test.find_by(id: params[:id])
      end
    end
  end
end
