module Api
  module V1
    class DiagnosticController < ApplicationController
      before_action :authenticate_user!
      before_action :verify_admin
      skip_before_action :verify_authenticity_token

      layout false

      REFEREE_NOT_FOUND = 'Referee not found, please check the email provided'.freeze

      def search
        if (referee = find_referee)
          json_string = RefereeSerializer.new(
            referee,
            include: %i[referee_certifications national_governing_bodies test_attempts test_results],
            params: { current_user: current_user, include_tests: true }
          ).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: REFEREE_NOT_FOUND }, status: :not_found
        end
      end

      def update_payment
        referee = User.find_by(id: permitted_params[:referee_id])

        if referee.update!(submitted_payment_at: permitted_params[:submitted_payment_at])
          json_string = RefereeSerializer.new(
            referee,
            include: %i[referee_certifications national_governing_bodies test_attempts test_results],
            params: { current_user: current_user, include_tests: true }
          ).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: @referee.errors.message }, status: :unprocessable_entity
        end
      end

      private

      def permitted_params
        params.permit(:referee_search, :submitted_payment_at, :referee_id)
      end

      def find_referee
        search_query = permitted_params.delete(:referee_search)

        User.find_by email: search_query
      end
    end
  end
end
