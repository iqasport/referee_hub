require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::NationalGoverningBodyTeamsController, type: :controller do
  describe 'GET #index' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let!(:teams) { create_list :team, 5, national_governing_body: ngb }
    let!(:other_teams) { create_list :team, 5 }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :index }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the teams associated with the current user ngb' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 5
    end

    it 'returns a meta' do
      subject

      response_data = JSON.parse(response.body)['meta']['total']

      expect(response_data).to eq 5
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'POST #create' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let(:body_data) do
      {
        name: 'DCQC',
        status: 'competitive',
        group_affiliation: 'community',
        city: 'Washington',
        state: 'DC',
        country: 'USA',
        urls: ['www.facebook.com/dcqc', 'www.twitter.com/dcqc']
      }
    end

    before do
      ngb.admins << user
      sign_in user
    end

    subject { post :create, params: body_data }

    context 'with valid params' do
      let(:team) { ngb.reload.teams.last }

      it 'returns http success' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'creates a new team' do
        subject

        expect(team.name).to eq 'DCQC'
        expect(team.status).to eq 'competitive'
        expect(team.group_affiliation).to eq 'community'
        expect(team.city).to eq 'Washington'
        expect(team.state).to eq 'DC'
        expect(team.country).to eq 'USA'
        expect(team.social_accounts.count).to eq 2
      end
    end

    context 'it handles an error' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, Team
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'GET #show' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let!(:team) { create :team, national_governing_body: ngb }
    let(:body_data) { { id: team.id } }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :show, params: body_data }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the correct team' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq team.id.to_s
    end

    context 'when the team exists but does not belong to the ngb' do
      let(:team) { create :team }

      it 'returns http not found' do
        subject

        expect(response).to have_http_status(:not_found)
      end
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'PUT #update' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let!(:team) { create :team, national_governing_body: ngb }
    let(:body_data) do
      {
        name: 'DCQC',
        id: team.id
      }
    end

    before do
      ngb.admins << user
      sign_in user
    end

    subject { put :update, params: body_data }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'updates the team' do
      subject

      expect(team.reload.name).to eq 'DCQC'
    end

    context 'with social accounts' do
      let(:body_data) do
        {
          id: team.id,
          urls: ['www.facebook.com/dcqc']
        }
      end

      it 'creates the passed in social account' do
        expect { subject }.to change { team.social_accounts.count }.by(1)
      end

      context 'when the team already has a social account' do
        let!(:social) { create :social_account, ownable_id: team.id, ownable_type: 'Team' }

        it 'removes the already existing account' do
          subject

          expect(team.social_accounts.count).to eq 1
          expect(team.social_accounts.first.url).to eq 'www.facebook.com/dcqc'
        end

        context "and it's includesd in the body data" do
          let(:url) { 'www.twitter.com/dcqc' }
          let!(:other_social) { create :social_account, ownable_id: team.id, ownable_type: 'Team', url: url }
          let(:body_data) do
            {
              id: team.id,
              urls: ['www.facebook.com/dcqc', url]
            }
          end

          it 'does not add the already existing account' do
            subject

            expect(team.social_accounts.count).to eq 2
          end
        end
      end
    end

    context 'it handles an error' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, Team
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'DELETE #destroy' do
    let(:id) { 123 }
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let!(:team) { create :team, national_governing_body: ngb, id: id }

    let(:body_data) { { id: team.id } }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { delete :destroy, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'destroys the team' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id'].to_i).to eq id
    end

    context 'it handles an error' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :destroy!, Team
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'POST #import' do
    include ActionDispatch::TestProcess

    let!(:user) { create :user, :ngb_admin }
    let!(:ngb) { create :national_governing_body }
    let(:service_double) { double(return_value: :perform) }

    before do
      ngb.admins << user
      sign_in user
      allow(Services::TeamCsvImport).to receive(:new).and_return(service_double)
      allow(service_double).to receive(:perform).and_return(true)
      @file = fixture_file_upload('import_test.csv', 'text/csv')
    end

    subject { post :import, params: { file: @file } }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'calls the team csv import service' do
      expect(Services::TeamCsvImport).to receive(:new).with(instance_of(String), ngb)

      subject
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end
end
