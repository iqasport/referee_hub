module Api
  module V1
    class CertificationsController < ApplicationController
      before_action :authenticate_user!
      skip_before_action :verify_authenticity_token

      layout false

      def index
        @certifications = Certification.all

        json_string = CertificationSerializer.new(@certifications).serialized_json

        render json: json_string, status: :ok
      end
    end
  end
end
