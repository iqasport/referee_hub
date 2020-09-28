require 'rails_helper'

RSpec.describe Services::FindAvailableUserTests do
  let!(:user) { create :user }
  let!(:assistant_cert_eighteen) { create :certification }
  let!(:assistant_cert_twenty) { create :certification, version: "twenty" }
  let!(:snitch_cert_eighteen) { create :certification, :snitch }
  let!(:snitch_cert_twenty) { create :certification, :snitch, version: "twenty" }
  let!(:head_cert_eighteen) { create :certification, :head }
  let!(:head_cert_twenty) { create :certification, :head, version: "twenty" }
  let!(:assistant_test_eighteen) do
    create :test, level: 'assistant', active: true, certification: assistant_cert_eighteen
  end
  let!(:assistant_test_twenty) do
    create :test, level: 'assistant', active: true, certification: assistant_cert_twenty
  end
  let!(:snitch_test_eighteen) do
    create :test, level: 'snitch', active: true, certification: snitch_cert_eighteen
  end
  let!(:snitch_test_twenty) do
    create :test, level: 'snitch', active: true, certification: snitch_cert_twenty
  end
  let!(:head_test_eighteen) do
    create :test, level: 'head', active: true, certification: head_cert_eighteen
  end
  let!(:head_test_twenty) do
    create :test, level: 'head', active: true, certification: head_cert_twenty
  end

  subject { described_class.new(user).perform }

  it 'returns the assistant tests for all versions when ref has no certs' do
    expect(subject.pluck(:id)).to include(assistant_test_eighteen.id, assistant_test_twenty.id)
  end

  context 'with existing assistant certs' do
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:assistant_twenty) { create :referee_certification, referee: user, certification: assistant_cert_twenty }

    it 'returns the snitch tests' do
      expect(subject.pluck(:id)).to include(snitch_test_eighteen.id, snitch_test_twenty.id)
    end
  end

  context 'with existing snitch certs and certification payment' do
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:assistant_twenty) { create :referee_certification, referee: user, certification: assistant_cert_twenty }
    let!(:snitch_eighteen) { create :referee_certification, referee: user, certification: snitch_cert_eighteen }
    let!(:snitch_twenty) { create :referee_certification, referee: user, certification: snitch_cert_twenty }
    let!(:eighteen_payment) { create :certification_payment, user: user, certification: head_cert_eighteen }
    let!(:twenty_payment) { create :certification_payment, user: user, certification: head_cert_twenty }

    it 'returns the head tests' do
      expect(subject.pluck(:id)).to include(head_test_eighteen.id, head_test_twenty.id)
    end
  end

  context 'with varying version certs' do
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:snitch_eighteen) { create :referee_certification, referee: user, certification: snitch_cert_eighteen }
    let!(:eighteen_payment) { create :certification_payment, user: user, certification: head_cert_eighteen }

    it 'returns the head test for one version and the assistant test for the other' do
      expect(subject.pluck(:id)).to include(head_test_eighteen.id, assistant_test_twenty.id)
    end
  end

  context 'with a test attempt within the cool down period' do
    let(:test_attempt) { create :test_attempt, test_level: 'assistant', test: assistant_test_eighteen, referee: user }

    it 'does not return the test with a recent cool down period' do
      expect(subject.pluck(:id)).to include(assistant_test_twenty.id)
    end
  end

  context 'without head ref payment' do
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:snitch_eighteen) { create :referee_certification, referee: user, certification: snitch_cert_eighteen }

    it 'only returns the assistant test' do
      expect(subject.pluck(:id)).to include(assistant_test_twenty.id)
    end
  end

  context 'with all certs in one version' do
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:snitch_eighteen) { create :referee_certification, referee: user, certification: snitch_cert_eighteen }
    let!(:head_eighteen) { create :referee_certification, referee: user, certification: head_cert_eighteen }

    it 'only returns the assistant twenty test' do
      expect(subject.pluck(:id)).to include(assistant_test_twenty.id)
      expect(subject.pluck(:id).length).to eq 1
    end
  end

  context 'with recertification' do
    let!(:test_attempt) do
      create(:test_attempt,
        test_level: 'assistant',
        test: assistant_test_eighteen,
        referee: user,
        created_at: 10.days.ago
      )
    end
    let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }
    let!(:recert_assistant) do
      create :test, level: 'assistant', active: true, certification: assistant_cert_twenty, recertification: true
    end

    before { allow_any_instance_of(TestAttempt).to receive(:in_cool_down_period?).and_return(false) }

    it 'returns the snitch eighteen and recert twenty' do
      expect(subject.pluck(:id).length).to eq 2
      expect(subject.pluck(:id)).to include(recert_assistant.id, snitch_test_eighteen.id)
    end
  end
end
