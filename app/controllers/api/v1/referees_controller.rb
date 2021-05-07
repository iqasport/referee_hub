module Api
  module V1
    class RefereesController < ApplicationController # rubocop:disable Metrics/ClassLength
      before_action :authenticate_user!, only: :update
      before_action :verify_ngb_or_iqa_admin, only: :export
      before_action :verify_ngb_or_iqa_admin_or_user
      before_action :find_referee, only: %i[show update tests]
      skip_before_action :verify_authenticity_token

      layout false

      UNAUTHORIZED_UPDATE = 'This account is unable to be updated'.freeze

      def index
        page = params[:page] || 1
        @referees = find_referees_from_filter
        referee_total = @referees.count
        @referees = @referees.page(page)

        json_string = RefereeSerializer.new(
          @referees,
          include: %i[referee_certifications referee_locations referee_teams teams certifications national_governing_bodies],
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
        raise StandardError if current_user.id != @referee.id

        update_national_governing_bodies(params.delete(:ngb_data))
        update_teams(params.delete(:teams_data))

        @referee.update!(permitted_params)
        json_string = RefereeSerializer.new(@referee, serializer_options).serialized_json

        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        error_message = @referee.errors.present? ? @referee.errors.messages : UNAUTHORIZED_UPDATE
        render json: { error: error_message }, status: :unprocessable_entity
      end

      def export
        export_options = search_params.presence || { national_governing_bodies: [current_user.owned_ngb&.first.id] }
        enqueued_job = ExportCsvJob.perform_later(
          current_user,
          'ExportedCsv::RefereeExport',
          export_options.to_json
        )

        render json: { data: { job_id: enqueued_job.provider_job_id } }, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: "Error exporting referees: #{e}" }, status: :unprocessable_entity
      end

      def tests
        tests = @referee.available_tests

        json_string = TestSerializer.new(tests, include: [:certification]).serialized_json
        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: e.message }, status: :unprocessable_entity
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
          include: %i[referee_certifications certifications national_governing_bodies test_attempts test_results referee_locations referee_teams teams],
          params: { current_user: current_user, include_tests: true, include_associations: true }
        }
      end
    end
  end
end
