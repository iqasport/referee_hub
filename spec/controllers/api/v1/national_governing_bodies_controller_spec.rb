require 'rails_helper'

RSpec.describe Api::V1::NationalGoverningBodiesController, type: :controller do
  describe 'GET #index' do
    let!(:national_governing_bodies) { create_list :national_governing_body, 3 }

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
    let!(:national_governing_body) { create :national_governing_body }

    subject { get :show, params: { id: national_governing_body.id } }

    it 'returns http success' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the passed id ref data' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
      expect(response_data[0]['attributes']['name']).to eq national_governing_body.name
      expect(response_data[0]['attributes']['website']).to eq national_governing_body.website
    end
  end
end
