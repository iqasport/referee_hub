require 'rails_helper'

RSpec.describe Api::V1::DiagnosticController, type: :controller do
  let(:admin) { create :user, :iqa_admin }
  let!(:search_ref) { create :user, :referee }

  describe 'POST #search' do
    before { sign_in admin }

    subject { post :search, params: body_data }

    context 'when the search includes an existing referee' do
      let(:body_data) { { referee_search: search_ref.email } }

      it 'returns http success' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'returns the searched referee' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data['id'].to_i).to eq search_ref.id
      end
    end

    context 'when the search fails' do
      let(:body_data) { { referee_search: 'nonsense@email.com' } }
      let(:expected_error) { described_class::REFEREE_NOT_FOUND }

      it 'returns not found error' do
        subject

        expect(response).to have_http_status(:not_found)
      end

      it 'returns an error message' do
        subject

        response_data = JSON.parse(response.body)['error']

        expect(response_data).to eq expected_error
      end
    end
  end

  describe 'PATCH #update_payment' do
    before { sign_in admin }

    subject { patch :update_payment, params: body_data }

    context 'it updates the payment for the passed referee' do
      let(:body_data) { { referee_id: search_ref.id, submitted_payment_at: Time.zone.now } }

      it 'returns http success' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'updates the payment' do
        expect { subject }.to change { search_ref.reload.submitted_payment_at }.from(nil)
      end
    end
  end

  context 'when signed in referee is not an admin' do
    let(:other_ref) { create :user, :referee }
    let(:body_data) { { referee_search: search_ref.email } }
    let(:expected_error) { ApplicationController::USER_UNAUTHORIZED }

    before { sign_in other_ref }

    subject { post :search, params: body_data }

    it 'returns an error' do
      subject

      expect(response).to have_http_status(:unauthorized)
    end

    it 'returns an error message' do
      subject

      response_data = JSON.parse(response.body)['error']

      expect(response_data).to eq expected_error
    end
  end
end
