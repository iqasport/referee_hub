module Api
  module V1
    class NationalGoverningBodyTeamsController < ApplicationController
      before_action :authenticate_user!
      before_action :verify_ngb_admin
      before_action :find_team, only: %i[show update destroy]
      skip_before_action :verify_authenticity_token
      layout false

      def index
        page = params[:page] || 1
        @teams = find_teams_from_filter
        teams_total = @teams.count
        @teams = @teams.page(page)

        json_string = TeamSerializer.new(@teams.order(:name), meta: { page: page, total: teams_total }).serialized_json

        render json: json_string, status: :ok
      end

      def create
        social_accounts = Services::CreateAndUpdateSocial.new(params.delete(:urls), :create).perform
        team = Team.new(permitted_params)
        team.national_governing_body = ngb_scope
        team.social_accounts = social_accounts
        team.save!

        json_string = TeamSerializer.new(team, include: [:social_accounts]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: team.errors.full_messages }, status: :unprocessable_entity
      end

      def show
        json_string = TeamSerializer.new(@team, include: [:social_accounts]).serialized_json

        render json: json_string, status: :ok
      end

      def update
        social_accounts = Services::CreateAndUpdateSocial.new(params.delete(:urls), :update, @team).perform
        @team.assign_attributes(permitted_params)
        @team.social_accounts << social_accounts if social_accounts.present?
        @team.save!

        json_string = TeamSerializer.new(@team, include: [:social_accounts]).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @team.errors.full_messages }, status: :unprocessable_entity
      end

      def destroy
        json_string = TeamSerializer.new(@team).serialized_json

        @team.destroy!
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: @team.errors.full_messages }, status: :unprocessable_entity
      end

      def import
        imported_ids = Services::TeamCsvImport.new(
          params['file'].tempfile.path,
          ngb_scope,
          params['mapped_headers']
        ).perform

        new_teams = ngb_scope.teams.where(id: imported_ids)
        page = params[:page] || 1
        teams_total = new_teams.count
        new_teams = new_teams.page(page)

        json_string = TeamSerializer.new(new_teams, meta: { page: page, total: teams_total }).serialized_json
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: exception.full_message }, status: :unprocessable_entity
      end

      def export
        export_options = search_params.presence || { national_governing_bodies: [ngb_scope.id] }
        enqueued_job = ExportCsvJob.perform_later(
          current_user,
          'ExportedCsv::TeamExport',
          export_options.to_json
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: "Error exporting teams: #{exception}" }, status: :unprocessable_entity
      end

      private

      def find_team
        @team = ngb_scope.teams.find(params[:id])
      end

      def ngb_scope
        @ngb_scope ||= current_user.owned_ngb.first
      end

      def find_teams_from_filter
        search_options = search_params.presence || { national_governing_bodies: [ngb_scope.id] }
        filter_results = Services::FilterTeams.new(search_options).filter

        if filter_results.respond_to?(:where)
          filter_results
        else
          Team.where(id: filter_results)
        end
      end

      def search_params
        params.permit(:q, national_governing_bodies: [], status: [], group_affiliation: [])
      end

      def permitted_params
        params.permit(:name, :group_affiliation, :status, :city, :country, :state, urls: [])
      end
    end
  end
end
