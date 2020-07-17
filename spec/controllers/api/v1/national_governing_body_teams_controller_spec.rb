require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::NationalGoverningBodyTeamsController, type: :controller do
  describe 'GET #index' do
    let!(:ngb) { create :national_governing_body }
    let!(:user) { create :user, :ngb_admin }
    let!(:teams) { create_list :team, 5, national_governing_body: ngb }
    let!(:other_teams) { create_list :team, 5 }
    let(:body_data) { { national_governing_bodies: [ngb.id], national_governing_body_id: ngb.id } }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :index, params: body_data }

    it_behaves_like 'it fails when a referee is not an admin'
    it_behaves_like 'it is a successful request'

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

    context 'with filters' do
      let(:q) { 'test qc' }
      let(:status) { ['inactive'] }
      let(:group_affiliation) { ['community'] }
      let(:body_data) do
        {
          national_governing_bodies: [ngb.id],
          q: q,
          status: status,
          group_affiliation: group_affiliation,
          national_governing_body_id: ngb.id
        }
      end

      context 'when searching by name' do
        let(:status) { nil }
        let(:group_affiliation) { nil }

        before { teams.first.update(name: q) }

        it 'only returns the matching team' do
          subject

          response_data = JSON.parse(response.body)['data']

          expect(response_data.length).to eq 1
          expect(response_data[0]['id'].to_i).to eq teams.first.id
        end
      end

      context 'when filtering by status' do
        let(:q) { nil }
        let(:group_affiliation) { nil }

        before { teams[1].update(status: 'inactive') }

        it 'only returns the matching team' do
          subject

          response_data = JSON.parse(response.body)['data']

          expect(response_data.length).to eq 1
          expect(response_data[0]['id'].to_i).to eq teams[1].id
        end
      end

      context 'when filtering by group_affiliation' do
        let(:q) { nil }
        let(:status) { nil }

        before { teams[2].update(group_affiliation: 'community') }

        it 'only returns the matching team' do
          subject

          response_data = JSON.parse(response.body)['data']

          expect(response_data.length).to eq 1
          expect(response_data[0]['id'].to_i).to eq teams[2].id
        end
      end
    end
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
        urls: ['www.facebook.com/dcqc', 'www.twitter.com/dcqc'],
        national_governing_body_id: ngb.id
      }
    end

    before do
      ngb.admins << user
      sign_in user
    end

    subject { post :create, params: body_data }

    context 'with valid params' do
      let(:team) { ngb.reload.teams.last }

      it_behaves_like 'it is a successful request'

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
    let(:body_data) { { id: team.id, national_governing_body_id: ngb.id } }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :show, params: body_data }

    it_behaves_like 'it is a successful request'

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
        id: team.id,
        national_governing_body_id: ngb.id
      }
    end

    before do
      ngb.admins << user
      sign_in user
    end

    subject { put :update, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'updates the team' do
      subject

      expect(team.reload.name).to eq 'DCQC'
    end

    context 'with social accounts' do
      let(:body_data) do
        {
          id: team.id,
          urls: ['www.facebook.com/dcqc'],
          national_governing_body_id: ngb.id
        }
      end

      it 'creates the passed in social account' do
        expect { subject }.to change { team.social_accounts.count }.by(1)
      end

      context 'when the team already has a social account' do
        let!(:social) { create :social_account, ownable_id: team.id, ownable_type: 'Team' }

        it 'removes the already existing account' do
          subject

          expect(team.reload.social_accounts.count).to eq 1
          expect(team.reload.social_accounts.first.url).to eq 'www.facebook.com/dcqc'
        end

        context "and it's included in the body data" do
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

            expect(team.reload.social_accounts.count).to eq 2
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

    let(:body_data) { { id: team.id, national_governing_body_id: ngb.id } }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { delete :destroy, params: body_data }

    it_behaves_like 'it is a successful request'

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
    let(:mapped_headers) do
      {
        'name': 'name',
        'city': 'city',
        'country': 'country',
        'state': 'state',
        'age_group': 'age_group',
        'status': 'status',
        'url_1': 'url_1',
      }.to_json
    end

    before do
      ngb.admins << user
      sign_in user
      allow(Services::TeamCsvImport).to receive(:new).and_return(service_double)
      allow(service_double).to receive(:perform).and_return(true)
      @file = fixture_file_upload('import_test.csv', 'text/csv')
    end

    subject do
      post :import, params: { file: @file, mapped_headers: mapped_headers, national_governing_body_id: ngb.id }
    end

    it_behaves_like 'it is a successful request'

    it 'calls the team csv import service' do
      expect(Services::TeamCsvImport).to receive(:new).with(instance_of(String), ngb, mapped_headers)

      subject
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'GET #export' do
    let!(:user) { create :user, :ngb_admin }
    let!(:ngb) { create :national_governing_body }
    let!(:teams) { create_list :team, 5, national_governing_body: ngb }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :export, params: { national_governing_body_id: ngb.id } }

    it 'should enqueue an export csv job' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['job_id']).to_not be_nil
    end
  end
end
