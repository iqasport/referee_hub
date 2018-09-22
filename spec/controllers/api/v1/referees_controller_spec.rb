require 'rails_helper'

RSpec.describe Api::V1::RefereesController, type: :controller do
  describe 'GET #index' do
    let!(:referees) { create_list :referee, 3 }

    subject { get :index }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns all of the ref data' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 3
    end
  end

  describe 'GET #show' do
    let!(:referee) { create :referee }

    subject { get :show, params: { id: referee.id } }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the passed id ref data' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
      expect(response_data[0]['attributes']['first_name']).to eq referee.first_name
      expect(response_data[0]['attributes']['last_name']).to eq referee.last_name
      expect(response_data[0]['attributes']['bio']).to eq referee.bio
      expect(response_data[0]['attributes']['email']).to eq referee.email
      expect(response_data[0]['attributes']['pronouns']).to eq referee.pronouns
      expect(response_data[0]['attributes']['show_pronouns']).to eq referee.show_pronouns
    end
  end

  describe 'POST #update' do
    let!(:referee) { create :referee }
    let!(:national_governing_bodies) { create_list :national_governing_body, 3 }
    let(:ngb_ids) { national_governing_bodies.pluck(:id) }

    before { sign_in referee }

    subject { post :update, params: body_data }

    context 'with valid params' do
      let(:body_data) do
        {
          id: referee.id,
          first_name: 'Tester',
          last_name: 'Testerton',
          bio: 'I am a brand new referee',
          pronouns: 'She/Her',
          show_pronouns: true,
          national_governing_body_ids: ngb_ids
        }
      end

      it 'returns http success' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'updates the data on the referee record' do
        subject

        expect(referee.reload.first_name).to eq 'Tester'
        expect(referee.reload.last_name).to eq 'Testerton'
        expect(referee.reload.bio).to eq 'I am a brand new referee'
        expect(referee.reload.pronouns).to eq 'She/Her'
        expect(referee.reload.show_pronouns).to eq true
        expect(referee.reload.national_governing_bodies.pluck(:id)).to eq ngb_ids
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

    context 'when referee already has national governing bodies' do
      let(:body_data) do
        {
          id: referee.id,
          national_governing_body_ids: ngb_ids
        }
      end

      before { referee.national_governing_bodies << national_governing_bodies }

      context 'and passed ids is equal to already existing ids' do
        it "doesn't change the referee locations" do
          expect { subject }.to_not change { referee.national_governing_bodies }
        end
      end

      context 'and passed ids includes a new id' do
        let!(:new_ngb) { create :national_governing_body }
        let(:ngb_ids) { national_governing_bodies.pluck(:id) << new_ngb.id }

        it 'adds the new ngb while keeping the old ones' do
          subject

          expect(referee.reload.national_governing_bodies.count).to eq 4
          expect(referee.reload.national_governing_bodies.last.id).to eq new_ngb.id
        end
      end

      context 'and passed ids does not include an existing id' do
        let(:ngb_ids) { national_governing_bodies[0..1].pluck(:id) }

        it 'removes the id that is no longer there' do
          subject

          expect(referee.reload.national_governing_bodies.count).to eq 2
          expect(referee.reload.national_governing_bodies.pluck(:id)).to match_array ngb_ids
        end
      end
    end
  end
end
