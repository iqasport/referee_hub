module Api
  module V1
    class RefereeCertificationsController < ::ApplicationController
      before_action :authenticate_referee!
      before_action :find_referee_certification, only: :update
      skip_before_action :verify_authenticity_token

      layout false

      def index
        @referee_certifications = current_referee.referee_certifications

        json_string = RefereeCertificationSerializer.new(@referee_certifications).serialized_json

        render json: json_string, status: :ok
      end

      def update
        if @referee_certification.update!(permitted_params)
          json_string = RefereeCertificationSerializer.new(@referee_certification)

          render json: json_string, status: :ok
        else
          render json: { error: @referee_certification.errors.messages }, status: :unprocessable_entity
        end
      end

      private

      def find_referee_certification
        @referee_certification = current_referee.referee_certifications.find_by(id: params[:id])
      end

      def permitted_params
        params.permit(:needs_renewal_at)
      end
    end
  end
end
