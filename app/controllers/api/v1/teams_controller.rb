module Api
  module V1
    class TeamsController < ApplicationController
      before_action :authenticate_user!
      skip_before_action :verify_authenticity_token
      layout false

      def index
        page = params[:page]
        limit = params[:limit]
        @teams = find_teams_from_filter
        teams_total = @teams.count
        @teams = @teams.order(name: :asc)
        @teams = @teams.paginate(page: page.to_i, per_page: limit.to_i) if page.present? || limit.present?

        json_string = TeamSerializer.new(@teams, meta: { page: page, total: teams_total }).serialized_json

        render json: json_string, status: :ok
      end

      private

      def find_teams_from_filter
        filter_results = Services::FilterTeams.new(permitted_params).filter

        if filter_results.respond_to?(:where)
          filter_results
        else
          Team.where(id: filter_results)
        end
      end

      def permitted_params
        params.permit(:q, national_governing_bodies: [], status: [], group_affiliation: [])
      end
    end
  end
end
