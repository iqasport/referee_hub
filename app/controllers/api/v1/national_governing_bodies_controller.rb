module Api
  module V1
    class NationalGoverningBodiesController < ApplicationController
      before_action :find_ngb, only: :show

      def index
        @national_governing_bodies = NationalGoverningBody.all

        json_string = NationalGoverningBodySerializer.new(@national_governing_bodies).serialized_json

        render json: json_string, status: :ok
      end

      def show
        json_string = NationalGoverningBodySerializer.new(@ngb).serialized_json

        render json: json_string, status: :ok
      end

      private

      def find_ngb
        @ngb = NationalGoverningBody.find_by(id: params[:id])
      end
    end
  end
end
