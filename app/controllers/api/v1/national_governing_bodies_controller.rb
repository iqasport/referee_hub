module Api
  module V1
    class NationalGoverningBodiesController < ApplicationController
      before_action :authenticate_user!, only: %i[show update_logo]
      before_action :find_ngb, only: %i[show update_logo]
      before_action :verify_update_admin, only: :update_logo
      skip_before_action :verify_authenticity_token

      layout false

      def index
        @national_governing_bodies = NationalGoverningBody.all

        json_string = NationalGoverningBodySerializer.new(@national_governing_bodies, fields: { national_governing_body: [:name]}).serialized_json

        render json: json_string, status: :ok
      end

      def show
        json_string = NationalGoverningBodySerializer.new(@ngb, include: [:social_accounts, :stats]).serialized_json

        render json: json_string, status: :ok
      end

      def update_logo
        @ngb.logo.attach(params['logo'])

        json_string = NationalGoverningBodySerializer.new(@ngb, include: [:social_accounts, :stats]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: "Error updating logo: #{exception}" }, status: :unprocessable_entity
      end

      private

      def find_ngb
        @ngb = NationalGoverningBody.find_by(id: params[:id])
      end

      def verify_update_admin
        return true if @ngb.admins.pluck(:user_id).include?(current_user.id) || current_user.iqa_admin?

        render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
      end
    end
  end
end
