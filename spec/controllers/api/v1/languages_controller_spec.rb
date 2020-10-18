require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::LanguagesController, type: :controller do
  let!(:user) { create :user }
  let!(:languages) { create_list :language, 5 }

  describe 'GET #index' do
    before { sign_in user }

    subject { get :index }

    it_behaves_like 'it is a successful request'

    it 'returns all of the languages' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq languages.length
      expect(response_data[0]['id']).to eq languages.first.id.to_s
    end
  end
end
