module Api
  module V1
    class QuestionsController < ApplicationController
      before_action :authenticate_referee!
      before_action :verify_admin, only: %i[create show update destroy]
      before_action :find_question, only: %i[show update destroy]
      before_action :find_test
      skip_before_action :verify_authenticity_token

      layout false

      def index
        questions = @test.questions

        json_string = QuestionSerializer.new(questions, include: [:answers]).serialized_json

        render json: json_string, status: :ok
      end

      def create
        question = Question.new(permitted_params)

        question.test_id = @test.id
        question.save!

        json_string = QuestionSerializer.new(question).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: question.errors.full_messages }, status: :unprocessable_entity
      end

      def show
        json_string = QuestionSerializer.new(@question, include: [:answers]).serialized_json

        render json: json_string, status: :ok
      end

      def update
        @question.update!(permitted_params)

        json_string = QuestionSerializer.new(@question, include: [:answers]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @question.errors.full_messages }, status: :unprocessable_entity
      end

      def destroy
        json_string = QuestionSerializer.new(@question).serialized_json

        @question.destroy!
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @question.errors.full_messages }, status: :unprocessable_entity
      end

      private

      def find_test
        id_to_find = params[:test_id] || @question.test_id
        @test = Test.find_by(id: id_to_find)
      end

      def find_question
        @question = Question.find_by(id: params[:id])
      end

      def permitted_params
        params.permit(:description, :feedback, :points_available)
      end
    end
  end
end
