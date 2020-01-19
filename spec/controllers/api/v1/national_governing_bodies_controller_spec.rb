require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::NationalGoverningBodiesController, type: :controller do
  describe 'GET #index' do
    let!(:national_governing_bodies) { create_list :national_governing_body, 3 }

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

    subject { get :show, params: { id: national_governing_body.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the passed id ref data' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
      expect(response_data[0]['attributes']['name']).to eq national_governing_body.name
      expect(response_data[0]['attributes']['website']).to eq national_governing_body.website
    end
  end
end
