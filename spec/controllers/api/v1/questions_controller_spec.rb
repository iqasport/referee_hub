require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::QuestionsController, type: :controller do
  let(:admin) { create :user, :iqa_admin }
  let(:non_admin) { create :user }
  let!(:test) { create :test }

  describe 'GET #index' do
    let!(:questions) { create_list :question, 3, test_id: test.id }

    before { sign_in admin }

    subject { get :index, params: { test_id: test.id } }

    it_behaves_like 'it is a successful request'

    it 'returns all of the questions associated with the passed test' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq questions.length
    end

    describe 'when a non-admin referee makes a request' do
      before { sign_in non_admin }

      it_behaves_like 'it is a successful request'
    end
  end

  describe 'POST #create' do
    let(:body_data) do
      {
        description: 'This is a new question',
        feedback: 'You messed up',
        points_available: 2,
        test_id: test.id
      }
    end

    before { sign_in admin }

    subject { post :create, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'creates a new question for the passed test' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
      expect(response_data['feedback']).to eq body_data[:feedback]
      expect(response_data['points_available']).to eq body_data[:points_available]
      expect(response_data['test_id']).to eq body_data[:test_id]
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, Question
    end
  end

  describe 'GET #show' do
    let!(:question) { create :question, test: test }

    before { sign_in admin }

    subject { get :show, params: { id: question.id, test_id: test.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the requested question' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id'].to_i).to eq question.id
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'PUT/PATCH #update' do
    let!(:question) { create :question, test: test }
    let(:body_data) { { description: 'I am a new description', test_id: test.id, id: question.id } }

    before { sign_in admin }

    subject { put :update, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'updates with the passed attributes' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, Question
    end
  end

  describe 'DELETE #destroy' do
    let(:id) { 123 }
    let!(:question) { create :question, test: test, id: id }

    before { sign_in admin }

    subject { delete :destroy, params: { id: id, test_id: test.id } }

    it_behaves_like 'it is a successful request'

    it 'returns the destroyed object' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id'].to_i).to eq id
    end

    it 'is not returned with a test any more' do
      subject

      expect(test.questions.pluck(:id)).not_to include(id)
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end
end
