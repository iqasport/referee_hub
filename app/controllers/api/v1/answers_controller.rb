module Api
  module V1
    class AnswersController < ApplicationController
      before_action :authenticate_referee!
      before_action :verify_admin, only: %i[create show update destroy]
      before_action :find_question
      before_action :find_answer, only: %i[show update destroy]
      skip_before_action :verify_authenticity_token

      layout false

      def index
        answers = @question.answers

        json_string = AnswerSerializer.new(answers).serialized_json

        render json: json_string, status: :ok
      end

      def create
        answer = Answer.new(permitted_params)

        if @question.answers << answer
          json_string = AnswerSerializer.new(answer).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: @question.errors.full_messages }, status: :unprocessable_entity
        end
      end

      def show
        json_string = AnswerSerializer.new(@answer).serialized_json

        render json: json_string, status: :ok
      end

      def update
        if @answer.update!(permitted_params)
          json_string = AnswerSerializer.new(@answer).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: @answer.errors.full_messages }, status: :unprocessable_entity
        end
      end

      def destroy
        json_string = AnswerSerializer.new(@answer).serialized_json

        if @answer.destroy!
          render json: json_string, status: :ok
        else
          render json: { error: @answer.errors.full_messages }, status: :unprocessable_entity
        end
      end

      private

      def find_question
        @question = Question.find_by(id: params[:question_id])
      end

      def find_answer
        @answer = @question.answers.find_by(id: params[:id])
      end

      def permitted_params
        params.permit(:description, :correct)
      end
    end
  end
end