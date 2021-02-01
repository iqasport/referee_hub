require 'stripe'

module Services
  module Stripe
    class FetchProducts
      attr_reader :client
      attr_accessor :product_objects

      def initialize
        @client = ::Stripe::StripeClient.new
        @product_objects = []
      end

      def perform
        products, _resp = client.request do
          ::Stripe::Product.list
        end

        products.data.each do |product|
          prices = fetch_prices(product)
          new_product = format_product(product, prices)

          @product_objects.push(new_product)
        end

        product_objects
      end

      private

      def fetch_prices(product)
        prices, _resp = client.request do
          ::Stripe::Price.list({ product: product.id })
        end

        prices.data
      end

      def formatted_prices(prices)
        prices.map do |price|
          {
            id: price.id,
            unit_amount: price.unit_amount,
            currency: price.currency
          }
        end
      end

      def format_product(product, prices)
        {
          id: product.id,
          active: product.active,
          name: product.name,
          prices: formatted_prices(prices)
        }
      end
    end
  end
end
