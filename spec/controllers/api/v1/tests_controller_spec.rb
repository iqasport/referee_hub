require 'rails_helper'

RSpec.describe Api::V1::TestsController, type: :controller do
  let(:admin) { create :referee, admin: true }
  let!(:cert) { create :certification, :snitch }

  shared_examples 'it fails when a referee is not an admin' do
    let(:other_ref) { create :referee }
    let(:expected_error) { ApplicationController::REFEREE_UNAUTHORIZED }

    before { sign_in other_ref }

    it 'returns an error' do
      subject

      expect(response).to have_http_status(:unauthorized)
    end

    it 'returns an error message' do
      subject

      response_data = JSON.parse(response.body)['error']

      expect(response_data).to eq expected_error
    end
  end

  describe 'GET #index' do
    let!(:tests) { create_list :test, 3 }

    before { sign_in admin }

    subject { get :index }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns all of the current tests' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq tests.length
    end
  end

  describe 'POST #create' do
    let(:body_data) do
      {
        description: 'I am a test',
        language: 'English',
        level: 'snitch',
        minimum_pass_percentage: 85,
        name: 'Test Name',
        negative_feedback: 'This is negative',
        positive_feedback: 'This is positive',
        time_limit: 20
      }
    end

    before { sign_in admin }

    subject { post :create, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the created test' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
      expect(response_data['language']).to eq body_data[:language]
      expect(response_data['level']).to eq body_data[:level]
      expect(response_data['minimum_pass_percentage']).to eq body_data[:minimum_pass_percentage]
      expect(response_data['name']).to eq body_data[:name]
      expect(response_data['negative_feedback']).to eq body_data[:negative_feedback]
      expect(response_data['positive_feedback']).to eq body_data[:positive_feedback]
      expect(response_data['time_limit']).to eq body_data[:time_limit]
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'PUT #update' do
    let!(:test) { create :test }
    let(:body_data) { { id: test.id, name: 'This is a different name' } }

    before { sign_in admin }

    subject { put :update, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'updates the test' do
      expect { subject }.to change { test.reload.name }.to(body_data[:name])
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the update fails' do
      let(:body_data) { { id: test.id, name: 'new name' } }
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      before do
        allow_any_instance_of(Test).to receive(:update!).and_raise(StandardError)
        allow_any_instance_of(Test).to receive(:errors).and_return(message_double)
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
    end
  end

  describe 'GET #show' do
    let(:test) { create :test }
    let(:body_data) { { id: test.id } }

    before { sign_in admin }

    subject { get :show, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the requested test' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq test.id.to_s
    end
  end

  describe 'DELETE #destroy' do
    let(:test) { create :test }
    let(:body_data) { { id: test.id } }

    before { sign_in admin }

    subject { delete :destroy, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the data of the test that was deleted' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq test.id.to_s
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end
end
