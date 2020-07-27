require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::RefereesController, type: :controller do
  describe 'GET #index' do
    let!(:referees) { create_list :user, 3 }

    subject { get :index }

    it_behaves_like 'it is a successful request'

    it 'returns all of the ref data' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 3
    end

    it 'returns a meta' do
      subject

      response_data = JSON.parse(response.body)['meta']['total']

      expect(response_data).to eq 3
    end

    context 'when searching' do
      before { referees.first.update!(first_name: 'Test') }

      subject { get :index, params: { q: 'test' } }

      it 'only returns the matching referee' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data.length).to eq 1
        expect(response_data[0]['id'].to_i).to eq referees.first.id
      end
    end

    context 'when filtering by certification' do
      let!(:certification) { create :certification }

      before { referees.first.update!(certifications: [certification]) }

      subject { get :index, params: { certifications: ['assistant'] } }

      it 'only returns the matching referee' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data.length).to eq 1
        expect(response_data[0]['id'].to_i).to eq referees.first.id
      end

      context 'when a referee has more than one certification' do
        let(:snitch_certification) { create :certification, :snitch }

        before { referees.first.update!(certifications: [certification, snitch_certification]) }

        it 'should return both associations' do
          subject

          response_data = JSON.parse(response.body)['data']
          cert_data = response_data[0]['relationships']['certifications']['data']

          expect(response_data.length).to eq 1
          expect(cert_data.length).to eq 2
          expect(cert_data[0]['id'].to_i).to eq certification.id
          expect(cert_data[1]['id'].to_i).to eq snitch_certification.id
        end
      end
    end

    context 'when filtering by national governing body' do
      let!(:ngb) { create :national_governing_body }

      before { referees.first.update!(national_governing_bodies: [ngb]) }

      subject { get :index, params: { national_governing_bodies: [ngb.id] } }

      it 'only returns the matching referee' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data.length).to eq 1
        expect(response_data[0]['id'].to_i).to eq referees.first.id
      end

      context 'when a referee has more than one national governing body' do
        let!(:other_ngb) { create :national_governing_body }

        before { referees.first.update!(national_governing_bodies: [ngb, other_ngb]) }

        it 'returns both associations' do
          subject

          response_data = JSON.parse(response.body)['data']
          ngb_data = response_data[0]['relationships']['nationalGoverningBodies']['data']

          expect(response_data.length).to eq 1
          expect(ngb_data.length).to eq 2
          expect(ngb_data[0]['id'].to_i).to eq ngb.id
          expect(ngb_data[1]['id'].to_i).to eq other_ngb.id
        end
      end
    end
  end

  describe 'GET #show' do
    let!(:referee) { create :user }

    before { allow_any_instance_of(User).to receive(:pending_policies).and_return([]) }

    subject { get :show, params: { id: referee.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the passed id ref data' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
      expect(response_data[0]['attributes']['firstName']).to eq referee.first_name
      expect(response_data[0]['attributes']['lastName']).to eq referee.last_name
      expect(response_data[0]['attributes']['bio']).to eq referee.bio
      expect(response_data[0]['attributes']['pronouns']).to eq referee.pronouns
      expect(response_data[0]['attributes']['showPronouns']).to eq referee.show_pronouns
    end

    context 'when the referee signed in is the same as the referee being returned' do
      before { sign_in referee }

      it 'returns is_editable as true' do
        subject

        response_data = Array.wrap(JSON.parse(response.body)['data'])

        expect(response_data[0]['attributes']['isEditable']).to eq true
      end
    end

    context 'when the referee signed in is different than the one being shown' do
      let!(:other_ref) { create :user }

      before { sign_in other_ref }

      it 'returns is_editable as false' do
        subject

        response_data = Array.wrap(JSON.parse(response.body)['data'])

        expect(response_data[0]['attributes']['isEditable']).to eq false
      end
    end
  end

  describe 'POST #update' do
    let!(:referee) { create :user }
    let!(:ngbs) { create_list :national_governing_body, 3 }
    let(:ngb_ids) { national_governing_bodies.pluck(:id) }

    before do
      sign_in referee
      allow_any_instance_of(User).to receive(:pending_policies).and_return([])
    end

    subject { post :update, params: body_data }

    context 'with valid params' do
      let(:body_data) do
        {
          id: referee.id,
          first_name: 'Tester',
          last_name: 'Testerton',
          bio: 'I am a brand new referee',
          pronouns: 'She/Her',
          show_pronouns: true
        }
      end

      it_behaves_like 'it is a successful request'

      it 'updates the data on the referee record' do
        subject

        expect(referee.reload.first_name).to eq 'Tester'
        expect(referee.reload.last_name).to eq 'Testerton'
        expect(referee.reload.bio).to eq 'I am a brand new referee'
        expect(referee.reload.pronouns).to eq 'She/Her'
        expect(referee.reload.show_pronouns).to eq true
      end
    end

    context 'with invalid permitted params' do
      let(:body_data) do
        {
          id: referee.id,
          email: 'hello@example.com'
        }
      end

      it 'does not update the record' do
        subject

        expect(referee.reload.email).to_not eq 'hello@example.com'
      end
    end

    context 'with ngb params' do
      let(:ngb_data) { { [ngbs.first.id] => 'primary', [ngbs[1].id] => 'secondary' } }
      let(:body_data) do
        {
          id: referee.id,
          ngb_data: ngb_data
        }
      end

      it 'adds the two ngbs to the referee' do
        expect { subject }.to change { referee.national_governing_bodies.count }.from(0).to(2)
      end

      context 'when one is being updated and the other is being added' do
        before { RefereeLocation.create(referee: referee, national_governing_body: ngbs.first, association_type: 'other') }

        it 'updates the existing and adds the new' do
          subject

          expect(referee.referee_locations.where(association_type: 'primary').first.national_governing_body_id).to eq ngbs.first.id
          expect(referee.reload.referee_locations.where(association_type: 'secondary').first.national_governing_body_id).to eq ngbs[1].id
        end
      end

      context 'when an existing ngb is missing from the request' do
        let(:ngb_data) { { [ngbs[1].id] => 'secondary' } }

        before { RefereeLocation.create(referee: referee, national_governing_body: ngbs.first, association_type: 'primary') }

        it 'removes the ngb' do
          subject

          expect(referee.referee_locations.find_by(national_governing_body_id: ngbs.first.id)).to be_nil
          expect(referee.referee_locations.count).to eq 1
        end
      end

      context 'when ngb data is missing and there are existing teams' do
        let(:ngb_data) { {} }

        before do
          referee.referee_locations.create(national_governing_body: ngbs.first, association_type: 'primary')
          referee.referee_locations.create(national_governing_body: ngbs[1], association_type: 'secondary')
        end

        it 'deletes the existing ngb associations' do
          expect { subject }.to change { referee.reload.national_governing_bodies.count }.from(2).to(0)
        end
      end
    end

    context 'with teams params' do
      let!(:teams) { create_list :team, 2 }
      let(:teams_data) { { [teams.first.id] => 'player', [teams.last.id] => 'coach' } }
      let(:body_data) do
        {
          id: referee.id,
          teams_data: teams_data
        }
      end

      it 'adds the two teams to the referee' do
        expect { subject }.to change { referee.teams.count }.from(0).to(2)
      end

      context 'when one is being updated and the other is being added' do
        before { RefereeTeam.create(referee: referee, team: teams.first, association_type: 'coach') }

        it 'updates the existing and adds the new' do
          subject

          expect(referee.referee_teams.where(association_type: 'player').first.team_id).to eq teams.first.id
          expect(referee.reload.referee_teams.where(association_type: 'coach').first.team_id).to eq teams.last.id
        end
      end

      context 'when an existing team is missing from the request' do
        let(:teams_data) { { [teams.last.id] => 'coach' } }

        before { RefereeTeam.create(referee: referee, team: teams.first, association_type: 'player') }

        it 'removes the team' do
          subject

          expect(referee.referee_teams.find_by(team_id: teams.first.id)).to be_nil
          expect(referee.referee_teams.count).to eq 1
        end
      end

      context 'when teams data is missing and there are existing teams' do
        let(:teams_data) { {} }

        before do
          referee.referee_teams.create(team: teams.first, association_type: 'player')
          referee.referee_teams.create(team: teams.last, association_type: 'coach')
        end

        it 'deletes the existing team associations' do
          expect { subject }.to change { referee.reload.teams.count }.from(2).to(0)
        end
      end
    end

    context 'with an invalid user' do
      let(:body_data) { { id: referee.id, first_name: 'person' } }
      let(:other_user) { create :user }

      before { sign_in other_user }

      it 'is a failed request' do
        subject

        response_data = JSON.parse(response.body)['error']

        expect(response_data).to eq described_class::UNAUTHORIZED_UPDATE
      end
    end

    context 'when the update fails' do
      let(:body_data) { { id: referee.id, nonsense: 'nonsense' } }
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :update!, User
    end
  end

  describe 'GET #export' do
    let!(:user) { create :user, :ngb_admin }
    let!(:ngb) { create :national_governing_body }
    let!(:referees) { create_list :user, 5, national_governing_bodies: [ngb] }

    before do
      ngb.admins << user
      sign_in user
    end

    subject { get :export }

    it 'should enqueue an export csv job' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['job_id']).to_not be_nil
    end

    context 'with iqa admin' do
      let!(:iqa_user) { create :user, :iqa_admin }

      before { sign_in iqa_user }

      subject { get :export, params: { national_governing_bodies: [ngb.id] } }

      it 'should enqueue an export csv job' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data['job_id']).to_not be_nil
      end
    end
  end

  describe 'GET #tests' do
    let(:referee) { create :user }
    let(:snitch) { create :certification, :snitch }
    let!(:test) { create :test, level: 'assistant', active: true }
    let!(:other_test) { create :test, certification: snitch }

    before { sign_in referee }

    subject { get :tests, params: { id: referee.id } }

    it_behaves_like 'it is a successful request'

    it 'returns only the tests for that user' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 1
      expect(response_data[0]['id'].to_i).to eq test.id
    end

    context 'when the test fetch fails' do
      let(:body_data) { { id: referee.id, nonsense: 'nonsense' } }
      let(:error_message) { ['I am an error'] }

      before do
        allow_any_instance_of(User).to receive(:available_tests).and_raise(StandardError)
        allow_any_instance_of(StandardError).to receive(:message).and_return(error_message)
        allow(Bugsnag).to receive(:notify).and_call_original
      end

      it 'returns an error' do
        subject

        expect(response).to have_http_status(:unprocessable_entity)
      end

      it 'returns an error message' do
        subject

        response_data = JSON.parse(response.body)['error']

        expect(response_data).to eq error_message
      end

      it 'calls bugsnag notify' do
        expect(Bugsnag).to receive(:notify).at_least(:once)

        subject
      end
    end
  end
end
