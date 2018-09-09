require 'rails_helper'

RSpec.describe Api::V1::RefereesController, type: :controller do
  describe "GET #index" do
    let!(:referees) { create_list :referee, 3 }

    it 'returns http success' do
      get :index

      expect(response).to have_http_status(:success)
    end

    it 'returns all of the ref data' do
      get :index

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq 3
    end
  end

  describe "GET #show" do
    let!(:referee) { create :referee }

    it 'returns http success' do
      get :show, params: { id: referee.id }

      expect(response).to have_http_status(:success)
    end

    it 'returns the passed id ref data' do
      get :show, params: { id: referee.id }

      response_data = Array.wrap(JSON.parse(response.body)['data'])

      expect(response_data.length).to eq 1
    end
  end

  describe "GET #update" do
    pending "returns http success" do
      get :update
      expect(response).to have_http_status(:success)
    end
  end
end
