require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::UsersController, type: :controller do
  let!(:current_user) { create :user }

  describe 'GET #get_current_user' do
    before { sign_in current_user }

    subject { get :get_current_user }

    it_behaves_like 'it is a successful request'

    it 'returns the current user' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq current_user.id.to_s
      expect(response_data['attributes']['firstName']).to eq current_user.first_name
    end

    context 'when user is not logged in' do
      let(:expected_error) { 'Not logged in' }

      before { sign_out current_user }

      it 'returns an error' do
        subject

        expect(response).to have_http_status(:unprocessable_entity)
      end

      it 'returns an error message' do
        subject

        response_data = JSON.parse(response.body)['error']

        expect(response_data).to eq expected_error
      end
    end
  end

  describe 'POST #accept_policies' do
    before do
      sign_in current_user
      current_user.reject_all_policies!
    end

    subject { post :accept_policies, params: { id: current_user.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the updated pending policy data' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['hasPendingPolicies']).to eq false
    end
  end

  describe 'POST #reject_policies' do
    before { sign_in current_user }

    subject { post :reject_policies, params: { id: current_user.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the updated pending policy data' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['hasPendingPolicies']).to eq false
    end
  end

  describe 'POST #update_avatar' do
    before do
      sign_in current_user
      @file = fixture_file_upload('logo.jpg', 'image/jpg')
    end

    subject { post :update_avatar, params: { id: current_user.id, avatar: @file } }

    it_behaves_like 'it is a successful request'

    it 'attachs the avatar to the user' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data[0]['attributes']['avatarUrl']).to_not be_nil
    end

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :avatar, User
    end
  end

  describe 'PATCH #update' do
    let!(:language) { create :language }

    before { sign_in current_user }

    subject { patch :update, params: { id: current_user.id, language_id: language.id } }

    it_behaves_like 'it is a successful request'

    it 'updates the user language' do
      expect { subject }.to change { current_user.reload.language }.from(nil).to(language)
    end
  end
end
