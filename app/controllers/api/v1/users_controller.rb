module Api
  module V1
    class UsersController < ApplicationController
      before_action :authenticate_user!, only: %w[accept_policies reject_policies update_avatar update]
      before_action :find_user, only: %w[accept_policies reject_policies update_avatar update]
      skip_before_action :verify_authenticity_token

      layout false

      def get_current_user
        raise StandardError, 'Not logged in' unless current_user.present?

        json_string = UserSerializer.new(current_user, serializer_options).serialized_json

        render json: json_string, status: :ok
      rescue => e
        render json: { error: e }, status: :unprocessable_entity
      end

      def accept_policies
        @user.confirm_all_policies!

        json_string = UserSerializer.new(@user, serializer_options).serialized_json

        render json: json_string, status: :ok
      end

      def reject_policies
        @user.reject_all_policies!

        json_string = UserSerializer.new(@user, serializer_options).serialized_json

        render json: json_string, status: :ok
      end

      def update_avatar
        @user.avatar.attach(params['avatar'])

        json_string = UserSerializer.new(@user, serializer_options).serialized_json

        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @user.errors.full_messages }, status: :unprocessable_entity
      end

      def update
        @user.update!(permitted_params)

        json_string = UserSerializer.new(@user, serializer_options).serialized_json

        render json: json_string, status: :ok
      rescue => e
        Bugsnag.notify(e)
        render json: { error: @user.errors.full_messages }, status: :unprocessable_entity
      end

      private

      def find_user
        @user = User.find(params[:id])
      end

      def serializer_options
        @serializer_options ||= {
          include: %i[roles certification_payments language],
          params: { current_user: current_user }
        }
      end

      def permitted_params
        params.permit(:language_id)
      end
    end
  end
end
