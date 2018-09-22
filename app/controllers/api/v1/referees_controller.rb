module Api
  module V1
    class RefereesController < ApplicationController
      before_action :find_referee, only: %i[show update]

      layout false

      def index
        @referees = Referee.all

        json_string = RefereeSerializer.new(@referees).serialized_json

        render json: json_string
      end

      def show
        json_string = RefereeSerializer.new(@referee, {include: [:national_governing_bodies, :certifications]}).serialized_json

        render json: json_string
      end

      def update
      end

      private

      def find_referee
        @referee = Referee.find(params[:id])
      end

      def permitted_params
        params.permit(:first_name, :last_name, :bio, :pronouns, :show_pronouns)
      end
    end
  end
end
