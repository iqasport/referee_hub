module Api
  module V1
    class UsersController < ApplicationController
      before_action :authenticate_user!
      before_action :find_user
      skip_before_action :verify_authenticity_token

      layout false

      def accept_policies
        @user.confirm_all_policies!

        json_string = RefereeSerializer.new(@user, params: { current_user: current_user }).serialized_json

        render json: json_string, status: :ok
      end

      def reject_policies
        @user.reject_all_policies!

        json_string = RefereeSerializer.new(@user, params: { current_user: current_user }).serialized_json

        render json: json_string, status: :ok
      end

      private

      def find_user
        @user = User.find(params[:id])
      end
    end
  end
end
