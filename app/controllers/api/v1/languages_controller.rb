module Api
  module V1
    class LanguagesController < ApplicationController
      before_action :authenticate_user!
      skip_before_action :verify_authenticity_token

      layout false

      def index
        languages = Language.all

        json_string = LanguageSerializer.new(languages)

        render json: json_string, status: :ok
      end
    end
  end
end
