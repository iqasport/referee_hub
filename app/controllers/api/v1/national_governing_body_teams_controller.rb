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
        @teams = ngb_scope.teams
        teams_total = @teams.count
        @teams = @teams.page(page)

        json_string = TeamSerializer.new(@teams, meta: { page: page, total: teams_total }).serialized_json

        render json: json_string, status: :ok
      end

      def create
        social_accounts = create_social_account_attrs # setting this variable here so the urls get removed from params
        team = Team.new(permitted_params)
        team.national_governing_body = ngb_scope
        team.social_accounts = social_accounts
        team.save!

        json_string = TeamSerializer.new(team).serialized_json

        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: team.errors.full_messages }, status: :unprocessable_entity
      end

      def show
        json_string = TeamSerializer.new(@team).serialized_json

        render json: json_string, status: :ok
      end

      def update
        social_accounts = update_social_accounts
        @team.assign_attributes(permitted_params)
        @team.social_accounts = social_accounts if social_accounts.present?
        @team.save!

        json_string = TeamSerializer.new(@team).serialized_json

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
        Services::TeamCsvImport.new(params['file'].tempfile.path, ngb_scope).perform

        new_teams = ngb_scope.teams
        page = params[:page] || 1
        teams_total = new_teams.count
        new_teams = new_teams.page(page)

        json_string = TeamSerializer.new(new_teams, meta: { page: page, total: teams_total }).serialized_json
        render json: json_string, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: exception.full_message }, status: :unprocessable_entity
      end

      private

      def find_team
        @team = ngb_scope.teams.find(params[:id])
      end

      def ngb_scope
        @ngb_scope ||= current_user.owned_ngb.first
      end

      def create_social_account_attrs
        return unless (urls = params.delete(:urls))

        generate_social_accounts(urls)
      end

      def update_social_accounts
        return nil unless (updated_urls = params.delete(:urls))

        existing_urls = @team.social_accounts.pluck(:url)
        urls_to_remove = existing_urls - updated_urls
        urls_to_add = existing_urls.concat(updated_urls).uniq - urls_to_remove

        SocialAccount.where(url: urls_to_remove).destroy_all if urls_to_remove.present?

        generate_social_accounts(urls_to_add)
      end

      def generate_social_accounts(urls)
        urls.map do |url|
          SocialAccount.new(
            url: url,
            account_type: SocialAccount.match_account_type(url)
          )
        end
      end

      def permitted_params
        params.permit(:name, :group_affiliation, :status, :city, :country, :state, urls: [])
      end
    end
  end
end
