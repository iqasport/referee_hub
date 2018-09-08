module Api
  module V1
    class RefereesController < ApplicationController
      layout false

      def index
        @referees = Referee.all

        json_string = RefereeSerializer.new(@referees).serialized_json

        render json: json_string
      end

      def show
      end

      def update
      end
    end
  end
end
