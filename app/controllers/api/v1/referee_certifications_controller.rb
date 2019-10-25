module Api
  module V1
    class RefereeCertificationsController < ::ApplicationController
      before_action :authenticate_referee!
      before_action :find_referee_certification, only: :update
      before_action :verify_admin, only: :create
      skip_before_action :verify_authenticity_token

      layout false

      def index
        @referee_certifications = current_referee.referee_certifications

        json_string = RefereeCertificationSerializer.new(@referee_certifications).serialized_json

        render json: json_string, status: :ok
      end

      def create
        certification = find_certification(permitted_params['level'])
        referee_certification = RefereeCertification.new(
          referee_id: permitted_params['referee_id'],
          certification: certification,
          received_at: permitted_params['received_at']
        )

        referee_certification.save!

        json_string = RefereeCertificationSerializer.new(referee_certification).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: referee_certification.errors.full_messages }, status: :unprocessable_entity
      end

      def update
        @referee_certification&.update!(permitted_update_params)

        json_string = RefereeCertificationSerializer.new(@referee_certification)

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @referee_certification.errors.full_messages }, status: :unprocessable_entity
      end

      private

      def find_referee_certification
        @referee_certification = current_referee.referee_certifications.find_by(id: params[:id])
      end

      def find_certification(level)
        Certification.find_by(level: level)
      end

      def permitted_params
        params.permit(:referee_id, :level, :received_at)
      end

      def permitted_update_params
        params.permit(:needs_renewal_at)
      end
    end
  end
end
