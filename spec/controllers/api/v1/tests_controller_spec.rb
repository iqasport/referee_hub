require 'rails_helper'
require_relative '_shared_examples'

RSpec.describe Api::V1::TestsController, type: :controller do
  let(:admin) { create :user, :iqa_admin }
  let!(:snitch) { create :certification, :snitch }
  let!(:assistant) { create :certification }
  let!(:head) { create :certification, :head }
  let!(:tests) { [] }

  before do
    tests.push(create(:test, certification_id: snitch.id))
    tests.push(create(:test, certification_id: assistant.id))
    tests.push(create(:test, certification_id: head.id))
  end

  after { Certification.all.destroy_all }

  describe 'GET #index' do
    before do
      sign_in admin
    end

    subject { get :index }

    it_behaves_like 'it is a successful request'

    it 'returns all of the current tests' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq tests.length
    end

    context 'when active_only is true' do
      let(:params) { { active_only: true } }
      let(:active_test) { tests.first }

      before { active_test.update!(active: true) }

      subject { get :index, params: params }

      it_behaves_like 'it is a successful request'

      it 'only returns the active test' do
        subject

        response_data = JSON.parse(response.body)['data']

        expect(response_data.length).to eq 1
        expect(response_data[0]['id'].to_i).to eq active_test.id
      end
    end
  end

  describe 'POST #create' do
    let(:body_data) do
      {
        description: 'I am a test',
        level: 'snitch',
        minimum_pass_percentage: 85,
        name: 'Test Name',
        negative_feedback: 'This is negative',
        positive_feedback: 'This is positive',
        time_limit: 20,
        certification_id: snitch.id
      }
    end

    before { sign_in admin }

    subject { post :create, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'returns the created test' do
      subject

      response_data = JSON.parse(response.body)['data']['attributes']

      expect(response_data['description']).to eq body_data[:description]
      expect(response_data['language']).to eq body_data[:language]
      expect(response_data['level']).to eq body_data[:level]
      expect(response_data['minimumPassPercentage']).to eq body_data[:minimum_pass_percentage]
      expect(response_data['name']).to eq body_data[:name]
      expect(response_data['negativeFeedback']).to eq body_data[:negative_feedback]
      expect(response_data['positiveFeedback']).to eq body_data[:positive_feedback]
      expect(response_data['timeLimit']).to eq body_data[:time_limit]
      expect(response_data['certificationId']).to eq body_data[:certification_id]
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the request fails' do
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :save!, Test
    end
  end

  describe 'PUT #update' do
    let!(:test) { create :test, certification_id: snitch.id }
    let(:body_data) { { id: test.id, name: 'This is a different name' } }

    before { sign_in admin }

    subject { put :update, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'updates the test' do
      expect { subject }.to change { test.reload.name }.to(body_data[:name])
    end

    it_behaves_like 'it fails when a referee is not an admin'

    context 'when the update fails' do
      let(:body_data) { { id: test.id, name: 'new name' } }
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :update!, Test
    end
  end

  describe 'GET #show' do
    let(:test) { create :test, certification_id: snitch.id }
    let(:body_data) { { id: test.id } }

    before { sign_in admin }

    subject { get :show, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'returns the requested test' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq test.id.to_s
    end
  end

  describe 'DELETE #destroy' do
    let(:test) { create :test, certification_id: snitch.id }
    let(:body_data) { { id: test.id } }

    before { sign_in admin }

    subject { delete :destroy, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'returns the data of the test that was deleted' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['id']).to eq test.id.to_s
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'GET #start' do
    let(:tester_ref) { create :user }
    let(:question_count) { 2 }
    let(:test) { create :test, testable_question_count: question_count, certification: assistant }
    let!(:questions) { create_list(:question, 5, test: test) }
    let(:body_data) { { id: test.id } }

    before { sign_in tester_ref }

    subject { get :start, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'returns the correct amount of questions' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data.length).to eq question_count
    end

    context 'when the question fetch fails' do
      let(:body_data) { { id: test.id } }
      let(:error_message) { ['I am an error'] }
      let(:message_double) { double(full_messages: error_message) }

      it_behaves_like 'it reports to bugsnag on failure', :fetch_random_questions, Test
    end

    context 'when the test level is in the cool down period' do
      let!(:test_attempt) { create :test_attempt, test: test, referee: tester_ref, test_level: 'assistant' }
      let(:expected_error) { "#{described_class::INVALID_TEST_ATTEMPT} 24 hours" }

      before { allow(test_attempt).to receive(:in_cool_down_period?).and_return(true) }

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

    context 'when the referee has too many test attempts' do
      let!(:test_attempts) { create_list :test_attempt, 6, test: test, referee: tester_ref, test_level: test.level }
      let(:expected_error) { described_class::INVALID_TRY_COUNT }

      before { allow_any_instance_of(TestAttempt).to receive(:in_cool_down_period?).and_return(false) }

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

    context 'when the referee has more than the max attempts but they are older than a month' do
      let!(:old_attempts) do
        create_list :test_attempt, 4, test: test, referee: tester_ref, test_level: test.level, created_at: 2.months.ago
      end
      let!(:test_attempts) do
        create_list :test_attempt, 3, test: test, referee: tester_ref, test_level: test.level, created_at: 1.day.ago
      end

      before { allow_any_instance_of(TestAttempt).to receive(:in_cool_down_period?).and_return(false) }

      it_behaves_like 'it is a successful request'
    end
  end

  describe 'POST #finish' do
    let(:tester_ref) { create :user }
    let(:test) { create :test, certification_id: snitch.id }
    let(:questions) { create_list(:question, 5, test: test) }
    let(:started_at) { Time.now.utc }
    let(:finished_at) { Time.now.utc + 15.minutes }
    let(:referee_answers) do
      questions.map do |question|
        answer = question.answers.find_by(id: question.id * 12)
        { question_id: question.id, answer_id: answer.id }
      end
    end
    let(:body_data) do
      { id: test.id, started_at: started_at.to_s, finished_at: finished_at.to_s, referee_answers: referee_answers }
    end

    before do
      questions.each do |question|
        create(:answer, id: question.id * 12, question: question, correct: true)
        create_list(:answer, 3, question: question)
      end
      Timecop.freeze(Time.now.utc)
      sign_in tester_ref
    end

    subject { post :finish, params: body_data }

    it_behaves_like 'it is a successful request'

    it 'returns the generated grade job id' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['job_id']).to_not be_nil
    end

    context 'when the test grading fails' do
      let(:body_data) { { id: test.id } }
      let(:error_message) { 'Error grading test' }

      before do
        allow(GradeJob).to receive(:perform_later).and_raise(StandardError)
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
  end

  describe 'POST #import' do
    include ActionDispatch::TestProcess

    let(:service_double) { double(return_value: :perform) }
    let(:mapped_headers) do
      {
        'description': 'description',
        'feedback': 'feedback',
        'points_available': 'points_available',
        'answer_1': 'answer_1',
        'correct_answer': 'correct_answer'
      }.to_json
    end

    before do
      sign_in admin
      allow(Services::TestCsvImport).to receive(:new).and_return(service_double)
      allow(service_double).to receive(:perform).and_return(true)
      @file = fixture_file_upload('import_test.csv', 'text/csv')
    end

    subject { post :import, params: { file: @file, mapped_headers: mapped_headers, id: tests[0].id } }

    it_behaves_like 'it is a successful request'

    it 'calls the test csv import service' do
      expect(Services::TestCsvImport).to receive(:new).with(instance_of(String), tests[0], mapped_headers)

      subject
    end

    it_behaves_like 'it fails when a referee is not an admin'
  end

  describe 'GET #export' do
    let!(:user) { create :user, :iqa_admin }
    let!(:test) { create :test, certification_id: snitch.id }

    before do
      sign_in user
    end

    subject { get :export, params: { id: test.id } }

    it 'should enqueue an export csv job' do
      subject

      response_data = JSON.parse(response.body)['data']

      expect(response_data['job_id']).to_not be_nil
    end
  end
end
