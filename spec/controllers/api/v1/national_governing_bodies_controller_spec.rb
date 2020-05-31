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

    it 'attachs the logo to the ngb' do
      subject

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data[0]['attributes']['logoUrl']).to_not be_nil
    end
  end
end
