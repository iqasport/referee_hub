require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::NationalGoverningBodiesController, type: :controller do
  describe 'GET #index' do
    let!(:national_governing_bodies) { create_list :national_governing_body, 3 }
    let!(:user) { create :user }

    before { sign_in user }

    subject { get :index }

    it_behaves_like 'it is a successful request'

    it 'returns all of the ref data' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 3
    end
  end

  describe 'GET #show' do
    let!(:national_governing_body) { create :national_governing_body }
    let!(:user) { create :user }

    before { sign_in user }

    subject { get :show, params: { id: national_governing_body.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the passed id ngb data' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
      expect(response_data[0]['attributes']['name']).to eq national_governing_body.name
      expect(response_data[0]['attributes']['website']).to eq national_governing_body.website
    end
  end

  describe 'PUT #update' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let(:body_data) do
      {
        name: 'Genovia',
        id: ngb.id
      }
    end

    before do
      ngb.admins << user
      sign_in user
    end

    subject { put :update, params: body_data }

    it_behaves_like 'it is a successful request'
    it_behaves_like 'it fails when a referee is not an admin'

    it 'updates the ngb' do
      subject

      expect(ngb.reload.name).to eq 'Genovia'
    end

    context 'with social accounts' do
      let(:body_data) do
        {
          id: ngb.id,
          urls: ['www.facebook.com/genovia']
        }
      end

      it 'creates the passed in social account' do
        expect { subject }.to change { ngb.social_accounts.count }.by(1)
      end

      context 'when the ngb already has a social account' do
        let!(:social) { create :social_account, ownable_id: ngb.id, ownable_type: 'NationalGoverningBody' }

        it 'removes the already existing account' do
          subject

          expect(ngb.reload.social_accounts.count).to eq 1
          expect(ngb.reload.social_accounts.first.url).to eq 'www.facebook.com/genovia'
        end

        context "and it's included in the body data" do
          let(:url) { 'www.twitter.com/genovia' }
          let!(:other_social) { create :social_account, ownable_id: ngb.id, ownable_type: 'NationalGoverningBody', url: url }
          let(:body_data) do
            {
              id: ngb.id,
              urls: ['www.facebook.com/genovia', url]
            }
          end

          it 'does not add the already existing account' do
            subject

            expect(ngb.reload.social_accounts.count).to eq 2
          end
        end
      end
    end

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, NationalGoverningBody
    end
  end

  describe 'POST #update_logo' do
    let!(:national_governing_body) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }

    before do
      national_governing_body.admins << user
      sign_in user
      @file = fixture_file_upload('logo.jpg', 'image/jpg')
    end

    subject { post :update_logo, params: { id: national_governing_body.id, logo: @file } }

    it_behaves_like 'it is a successful request'
    it_behaves_like 'it fails when a referee is not an admin'

    it 'attachs the logo to the ngb' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data[0]['attributes']['logoUrl']).to_not be_nil
    end

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :logo, NationalGoverningBody
    end
  end
end
