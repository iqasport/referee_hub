module Api
  module V1
    class NationalGoverningBodiesController < ApplicationController
      before_action :authenticate_user!, only: %i[show update update_logo]
      before_action :find_ngb, only: %i[show update update_logo]
      before_action :verify_update_admin, only: %i[update update_logo]
      before_action :verify_valid_update_params, only: %i[update]
      before_action :verify_admin, only: %i[import]
      skip_before_action :verify_authenticity_token

      layout false

      def index
        page = params[:page] || 1
        @national_governing_bodies = NationalGoverningBody
          .includes(:social_accounts, :teams, :referees, :stats)
          .all
          .order(:name)
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
        return true if @ngb.is_admin?(current_user.id) || current_user.iqa_admin?

        render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
      end

      def verify_valid_update_params
        new_membership_status = permitted_params[:membership_status]
        new_region = permitted_params[:region]
        prev_membership_status = @ngb.membership_status
        prev_region = @ngb.region
        is_ngb_admin = @ngb.is_admin?(current_user.id)

        if ((new_membership_status == prev_membership_status) && (new_region == prev_region)) && is_ngb_admin
          return true
        end
        return true if (new_membership_status.blank? && new_region.blank?) && is_ngb_admin
        return true if (new_membership_status.present? || new_region.present?) && current_user.iqa_admin?

        render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
      end

      def permitted_params
        params.permit(:name, :acronym, :country, :player_count, :website, :region, :membership_status, urls: [])
      end
    end
  end
end
