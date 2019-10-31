require 'rails_helper'

RSpec.describe Api::V1::AnswersController, type: :controller do
  let(:admin) { create :referee, admin: true }
  let(:non_admin) { create :referee }
  let!(:test) { create :test }
  let!(:question) { create :question, test: test }

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

  shared_examples 'it reports to bugsnag on failure' do |method|
    before do
      allow_any_instance_of(Answer).to receive(method).and_raise(StandardError)
      allow_any_instance_of(Answer).to receive(:errors).and_return(message_double)
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

  describe 'GET #index' do
    let!(:answers) { create_list :answer, 3, question_id: question.id }
    let(:random) { nil }

    before { sign_in admin }

    subject { get :index, params: { question_id: question.id, random: random } }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns all of the answers associated with the passed question' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq answers.length
    end

    context 'when a non-admin referee makes a request' do
      before { sign_in non_admin }

      it 'is a successful request' do
        subject

        expect(response).to have_http_status(:successful)
      end
    end

    context 'when random is present' do
      let(:question) { create :question }
      let!(:answers) { create_list :answer, 3, question_id: question.id }
      let(:random) { true }

      before { allow(question).to receive(:randomize_answers) }

      it 'is a successful request' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'returns all of the answers associated with the passed question' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data.length).to eq answers.length
      end
    end
  end

  describe 'POST #create' do
    let(:body_data) do
      {
        description: 'This is a new answer',
        correct: true,
        question_id: question.id
      }
    end

    before { sign_in admin }

    subject { post :create, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'creates a new answer for the passed question' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
      expect(response_data['correct']).to eq body_data[:correct]
      expect(response_data['question_id']).to eq body_data[:question_id]
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!
    end
  end

  describe 'GET #show' do
    let!(:answer) { create :answer, question: question }

    before { sign_in admin }

    subject { get :show, params: { id: answer.id, question_id: question.id } }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'returns the requested question' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id'].to_i).to eq answer.id
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'PUT/PATCH #update' do
    let!(:answer) { create :answer, question: question }
    let(:body_data) { { description: 'I am a new description', question_id: question.id, id: answer.id } }

    before { sign_in admin }

    subject { put :update, params: body_data }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

    it 'updates with the passed attributes' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!
    end

    context 'when the question already has a correct answer' do
      let!(:other_answer) { create :answer, question: question, correct: true }
      let(:body_data) { { correct: true, question_id: question.id, id: answer.id } }

      it 'is a successful request' do
        subject

        expect(response).to have_http_status(:successful)
      end

      it 'updates the requested answer' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data['attributes']['correct']).to eq body_data[:correct]
        expect(response_data['id'].to_i).to eq answer.id
      end

      it 'updates the previously correct answer' do
        expect { subject }.to change { other_answer.reload.correct }.from(true).to(false)
      end
    end
  end

  describe 'DELETE #destroy' do
    let(:id) { 123 }
    let!(:answer) { create :answer, question: question, id: id }

    before { sign_in admin }

    subject { delete :destroy, params: { id: id, question_id: question.id } }

    it 'is a successful request' do
      subject

      expect(response).to have_http_status(:successful)
    end

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
