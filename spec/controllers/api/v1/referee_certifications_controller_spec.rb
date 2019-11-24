require 'rails_helper'

RSpec.describe Api::V1::RefereeCertificationsController, type: :controller do
  describe 'GET #index' do
    let!(:referee) { create :user, :referee }
    let!(:assistant) { create :referee_certification, referee: referee }
    let!(:snitch) { create :referee_certification, :snitch, referee: referee }
    let!(:head) { create :referee_certification, :head, referee: referee }

    before { sign_in referee }

    subject { get :index }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns all of the referees certifications' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 3
    end

    context 'when there is not a signed in ref' do
      before { sign_out referee }

      it 'redirects to the login' do
        subject

        expect(response).to have_http_status(:found)
      end
    end
  end

  describe 'POST #update' do
    let!(:referee) { create :user, :referee }
    let!(:assistant) { create :referee_certification, referee: referee }

    before { sign_in referee }

    subject { post :update, params: body_data }

    context 'with valid params' do
      let(:body_data) { { id: assistant.id, needs_renewal_at: Time.zone.now } }

      it 'returns http success' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'updates the referee certification' do
        subject

        expect(assistant.reload.needs_renewal_at).to_not be_nil
      end
    end

    context 'with invalid params' do
      let(:body_data) { { id: assistant.id, blah: 'this is nonsense' } }

      it 'does not update the record' do
        subject

        expect(assistant.reload.needs_renewal_at).to be_nil
      end
    end
  end
end
