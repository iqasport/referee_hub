module Api
  module V1
    class RefereesController < ApplicationController # rubocop:disable Metrics/ClassLength
      before_action :authenticate_user!, only: :update
      before_action :verify_ngb_or_iqa_admin, only: :export
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
          include: %i[referee_certifications referee_locations],
          params: { current_user: current_user, include_tests: false, include_associations: true },
          meta: { page: page, total: referee_total }
        ).serialized_json

        render json: json_string, status: :ok
      end

      def show
        json_string = RefereeSerializer.new(@referee, serializer_options).serialized_json
        render json: json_string, status: :ok
      end

      def update
        update_national_governing_bodies(params.delete(:ngb_data))
        update_teams(params.delete(:teams))

        if @referee.update!(permitted_params)
          json_string = RefereeSerializer.new(@referee, serializer_options).serialized_json

          render json: json_string, status: :ok
        else
          render json: { error: @referee.errors.messages }, status: :unprocessable_entity
        end
      end

      def export
        export_options = search_params.presence || { national_governing_bodies: [current_user.owned_ngb.first.id] }
        enqueued_job = ExportCsvJob.perform_later(
          user: current_user,
          type: 'ExportedCsv::RefereeExport',
          export_options: export_options.to_json
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => exception
        Bugsnag.notify(exception)
        render json: { error: "Error exporting referees: #{exception}" }, status: :unprocessable_entity
      end

      private

      def update_national_governing_bodies(ngb_data)
        current_ngb_ids = @referee.national_governing_bodies.pluck(:id)
        new_ngb_ids = ngb_data.present? ? ngb_data.keys : []
        ngbs_to_remove = current_ngb_ids - new_ngb_ids

        to_remove = @referee.referee_locations.where(national_governing_body_id: ngbs_to_remove)
        to_remove.destroy_all if to_remove.present?
        return if ngb_data.blank?

        ngb_data.each do |ngb_id, association|
          referee_location = RefereeLocation.find_or_initialize_by(referee: @referee, national_governing_body_id: ngb_id)
          referee_location.association_type = association

          referee_location.save!
        end
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
          :export_name
        )
      end

      def search_params
        params.permit(:q, certifications: [], national_governing_bodies: []).to_h.deep_symbolize_keys
      end

      def serializer_options
        @serializer_options ||= {
          include: %i[referee_certifications certifications national_governing_bodies test_attempts test_results referee_locations],
          params: { current_user: current_user, include_tests: true, include_associations: true }
        }
      end
    end
  end
end
