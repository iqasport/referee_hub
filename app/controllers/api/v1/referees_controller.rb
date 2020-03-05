module Api
  module V1
    class RefereesController < ApplicationController
      before_action :authenticate_user!, only: :update
      before_action :find_referee, only: %i[show update]
      skip_before_action :verify_authenticity_token

      layout false

      def index
        page = params[:page] || 1

        @referees = find_referees_from_filter

        referee_total = @referees.count
        @referees = @referees.page(page)

        json_string = RefereeSerializer.new(
          @referees,
          include: [:referee_certifications],
          params: { current_user: current_user, include_tests: false },
          meta: { page: page, total: referee_total }
        ).serialized_json

        render json: json_string, status: :ok
      end

      def show
        json_string = RefereeSerializer.new(@referee, serializer_options).serialized_json

        render json: json_string, status: :ok
      end

      def update
        national_governing_body_ids = params.delete(:national_governing_body_ids)
        update_national_governing_bodies(national_governing_body_ids) if national_governing_body_ids.present?
        update_teams(params.delete(:teams))

        if @referee.update!(permitted_params)
          json_string = RefereeSerializer.new(@referee, serializer_options).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: @referee.errors.messages }, status: :unprocessable_entity
        end
      end

      private

      def update_national_governing_bodies(ngb_ids)
        current_ngb_ids = @referee.national_governing_bodies.pluck(:id)
        national_governing_body_remove = current_ngb_ids - ngb_ids
        national_governing_body_add = ngb_ids - current_ngb_ids
        return unless national_governing_body_remove.present? || national_governing_body_add.present?

        to_remove = @referee.referee_locations.where(national_governing_body_id: national_governing_body_remove)
        to_remove.destroy_all if to_remove.present?

        ngb_records = NationalGoverningBody.where(id: national_governing_body_add)
        @referee.national_governing_bodies << ngb_records if ngb_records.present?
      end

      def update_teams(teams_data)
        current_team_ids = @referee.teams.pluck(:id)
        new_team_ids = teams_data.present? ? teams_data.keys : []
        teams_to_remove = current_team_ids - new_team_ids

        to_remove = @referee.referee_teams.where(team_id: teams_to_remove)
        to_remove.destroy_all if to_remove.present?
        return if teams_data.blank?

        teams_data.each do |team_id, position|
          referee_team = RefereeTeam.find_or_initialize_by(referee: @referee, team_id: team_id)
          referee_team.association_type = position

          referee_team.save!
        end
      end

      def find_referee
        @referee = User.find_by(id: params[:id])
      end

      def find_referees_from_filter
        filter_results = Services::FilterReferees.new(search_params).filter

        if filter_results.respond_to?(:where)
          filter_results
        else
          User.includes(:national_governing_bodies, :certifications).where(id: filter_results)
        end
      end

      def permitted_params
        params.permit(
          :id,
          :first_name,
          :last_name,
          :bio,
          :pronouns,
          :show_pronouns,
          :submitted_payment_at,
          :getting_started_dismissed_at
        )
      end

      def search_params
        params.permit(:q, certifications: [], national_governing_bodies: []).to_h.deep_symbolize_keys
      end

      def serializer_options
        @serializer_options ||= {
          include: %i[referee_certifications certifications national_governing_bodies test_attempts test_results],
          params: { current_user: current_user, include_tests: true }
        }
      end
    end
  end
end
