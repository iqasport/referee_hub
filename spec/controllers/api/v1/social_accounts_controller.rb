module Api
  module V1
    class SocialAccountsController < ApplicationController
      before_action :authenticate_user!
      skip_before_action :verify_authenticity_token

      layout false

      def index
        @social_accounts = find_accounts_from_filter

        json_string = SocialAccountSerializer.new(@social_accounts).serialized_json

        render json: json_string, status: :ok
      end

      private

      def find_accounts_from_filter
        filter_results = Services::Filter::SocialAccounts.new(permitted_params).filter

        if filter_results.respond_to?(:where)
          filter_results
        else
          SocialAccounts.where(id: filter_results)
        end
      end

      def permitted_params
        params.permit(team_ids: [], account_ids: [], ngb_ids: [])
      end
    end
  end
end
