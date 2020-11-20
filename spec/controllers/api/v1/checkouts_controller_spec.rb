require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::CheckoutsController, type: :controller do
  let(:user) { create :user }

  before { sign_in user }

  context 'GET #products' do
    let(:service_double) { double(:perform => :return_value) }
    let(:products) do
      [
        {
          id: 'pro_123456',
          active: true,
          name: 'Product 1',
          prices: [{
            id: 'pr_123456',
            unit_amount: 1500,
            currency: 'usd'
          }]
        },
        {
          id: 'pro_123457',
          active: true,
          name: 'Product 2',
          prices: [{
            id: 'pr_123457',
            unit_amount: 1500,
            currency: 'usd'
          }]
        }
      ]
    end

    before do
      allow(Services::Stripe::FetchProducts).to receive(:new).and_return(service_double)
      allow(service_double).to receive(:perform).and_return(products)
    end

    subject { get :products }

    it_behaves_like 'it is a successful request'

    it 'returns the products from stripe' do
      subject

      response_data = JSON.parse(response.body)

      expect(response_data[0]['id']).to eq products[0][:id]
      expect(response_data[1]['prices'][0]['id']).to eq products[1][:prices][0][:id]
    end
  end

  context 'GET #show' do
    let(:session) { { id: 'ssn_123456' } }

    before { allow(Stripe::Checkout::Session).to receive(:retrieve).and_return(session) }

    subject { get :show, params: { id: session[:id] } }

    it_behaves_like 'it is a successful request'

    it 'returns the fetched session' do
      subject

      response_data = JSON.parse(response.body)

      expect(response_data['id']).to eq session[:id]
    end
  end

  context 'POST #create' do
    let(:price) { 'pr_123456' }
    let(:cert_id) { '3' }
    let(:session_data) do
      {
        allow_promotion_codes: true,
        success_url: "http://localhost:3000/referees/#{user.id}/tests?status=success",
        cancel_url: "http://localhost:3000/referees/#{user.id}/tests?status=cancelled",
        payment_method_types: ['card'],
        mode: 'payment',
        line_items: [{ quantity: '1', price: price }],
        customer_email: user.email,
        metadata: {
          certification_id: cert_id,
        }
      }
    end
    let(:created_session) { 'ssn_123456' }

    before do
      allow(Stripe::Checkout::Session).to receive(:create).and_return(session_data.merge('id' => created_session))
    end

    subject { post :create, params: { price: price, certification_id: cert_id } }

    it_behaves_like 'it is a successful request'

    it 'returns the created session id' do
      expect(Stripe::Checkout::Session).to receive(:create).with(session_data)

      subject

      response_data = JSON.parse(response.body)

      expect(response_data['sessionId']).to eq created_session
    end
  end
end
