module Api
  module V1
    class NationalGoverningBodiesController < ApplicationController
      before_action :authenticate_user!, only: %i[show update update_logo]
      before_action :find_ngb, only: %i[show update update_logo]
      before_action :verify_update_admin, only: %i[update update_logo]
      skip_before_action :verify_authenticity_token

      layout false

      def index
        page = params[:page] || 1
        @national_governing_bodies = NationalGoverningBody.all.order(:name)
        ngbs_total = @national_governing_bodies.count
        @national_governing_bodies = @national_governing_bodies.page(page)

        json_string = NationalGoverningBodySerializer.new(
          @national_governing_bodies,
          meta: { page: page, total: ngbs_total },
          fields: { national_governing_body: [:name] }
        ).serialized_json

        render json: json_string, status: :ok
      end

      def show
        json_string = NationalGoverningBodySerializer.new(@ngb, include: [:social_accounts, :stats]).serialized_json

        render json: json_string, status: :ok
      end

      def update
        social_accounts = Services::CreateAndUpdateSocial.new(params.delete(:urls), :update, @ngb).perform
        @ngb.assign_attributes(permitted_params)
        @ngb.social_accounts << social_accounts if social_accounts.present?
        @ngb.save!

        json_string = NationalGoverningBodySerializer.new(@ngb, include: [:social_accounts, :stats]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @ngb.errors.full_messages }, status: :unprocessable_entity
      end

      def update_logo
        @ngb.logo.attach(params['logo'])

        json_string = NationalGoverningBodySerializer.new(@ngb, include: [:social_accounts, :stats]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @ngb.errors.full_messages }, status: :unprocessable_entity
      end

      def import
        imported_ids = Services::NgbCsvImport.new(
          params['file'].tempfile.path,
          params['mapped_headers']
        ).perform

        new_ngbs = NationalGoverningBody.where(id: imported_ids)
        page = params[:page] || 1
        ngbs_total = new_ngbs.count
        new_ngbs = new_ngbs.page(page)

        json_string = NationalGoverningBodySerializer.new(
          new_ngbs,
          meta: { page: page, total: ngbs_total }
        ).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: exception.full_message }, status: :unprocessable_entity
      end

      private

      def find_ngb
        @ngb = NationalGoverningBody.find_by(id: params[:id])
      end

      def verify_update_admin
        return true if @ngb.admins.pluck(:user_id).include?(current_user.id) || current_user.iqa_admin?

        render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
      end

      def permitted_params
        params.permit(:name, :acronym, :country, :player_count, :website, :region, urls: [])
      end
    end
  end
end
