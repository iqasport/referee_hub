require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::RefereeCertificationsController, type: :controller do
  describe 'GET #index' do
    let!(:referee) { create :user }
    let!(:assistant) { create :referee_certification, referee: referee }
    let!(:snitch) { create :referee_certification, :snitch, referee: referee }
    let!(:head) { create :referee_certification, :head, referee: referee }

    before { sign_in referee }

    subject { get :index }

    it_behaves_like 'it is a successful request'

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

  describe 'PUT #update' do
    let!(:referee) { create :user }
    let!(:assistant) { create :referee_certification, referee: referee }

    before { sign_in referee }

    subject { put :update, params: body_data }

    context 'with valid params' do
      let(:body_data) { { id: assistant.id, needs_renewal_at: Time.zone.now, referee_id: referee.id } }

      it_behaves_like 'it is a successful request'

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

  describe 'POST #create' do
    let(:admin) { create :user, :iqa_admin }
    let(:referee) { create :user }
    let(:certification) { create :certification }
    let(:body_data) { { certification_id: certification.id, referee_id: referee.id, received_at: DateTime.now.to_s } }

    before { sign_in admin }

    subject { post :create, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'creates a certification' do
      expect { subject }.to change { referee.certifications.count }.by(1)
    end
  end
end
