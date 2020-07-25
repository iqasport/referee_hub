module Api
  module V1
    class CheckoutsController < ApplicationController
      before_action :authenticate_user!
      skip_before_action :verify_authenticity_token

      def products
        product_objects = Services::Stripe::FetchProducts.new().perform()

        json_string = product_objects.to_json
        render json: json_string, status: :ok
      end

      def show
        session = Stripe::Checkout::Session.retrieve(params[:id])

        render json: session.to_json, status: :ok
      end

      def create
        url = Rails.env.production? ? 'https://www.iqareferees.org' : 'http://localhost:3000'
        session_data = {
          success_url: url + "/referees/#{current_user.id}/tests?status=success",
          cancel_url: url + "/referees/#{current_user.id}/tests?status=cancelled",
          payment_method_types: ['card'],
          mode: 'payment',
          line_items: [{ quantity: '1', price: params[:price] }],
          metadata: {
            certification_id: params[:certification_id],
          }
        }

        if current_user.stripe_customer_id.present?
          session_data.merge(customer: current_user.stripe_customer_id)
        else
          session_data.merge(customer_email: current_user.email)
        end

        session = Stripe::Checkout::Session.create(session_data)

        render json: { sessionId: session['id'] }.to_json, status: :ok
      end

      private

      def permitted_params
        params.permit(:certification_id, :price)
      end
    end
  end
end
