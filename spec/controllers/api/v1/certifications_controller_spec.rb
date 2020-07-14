require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::CertificationsController, type: :controller do
  let!(:user) { create :user }

  before do
    sign_in user
    create(:certification)
    create(:certification, :snitch)
    create(:certification, :head)
  end

  subject { get :index }

  it_behaves_like 'it is a successful request'

  it 'returns all 3 certifications' do
    subject

    response_data = JSON.parse(response.body)['data']

    expect(response_data.length).to eq 3
  end
end
