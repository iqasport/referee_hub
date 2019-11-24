shared_examples 'it fails when a referee is not an admin' do
  let(:other_ref) { create :user, :referee }
  let(:expected_error) { ApplicationController::USER_UNAUTHORIZED }

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

shared_examples 'it reports to bugsnag on failure' do |method, resource|
  before do
    allow_any_instance_of(resource).to receive(method).and_raise(StandardError)
    allow_any_instance_of(resource).to receive(:errors).and_return(message_double)
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
