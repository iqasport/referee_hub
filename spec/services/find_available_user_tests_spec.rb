require 'rails_helper'

RSpec.describe Services::FindAvailableUserTests do
  let!(:user) { create :user }
  let!(:assistant_cert_eighteen) { create :certification }
  let!(:assistant_cert_twenty) { create :certification, version: 'twenty' }
  let!(:assistant_cert_twentytwo) { create :certification, version: 'twentytwo' }
  let!(:snitch_cert_eighteen) { create :certification, :snitch }
  let!(:snitch_cert_twenty) { create :certification, :snitch, version: 'twenty' }
  let!(:head_cert_eighteen) { create :certification, :head }
  let!(:head_cert_twenty) { create :certification, :head, version: 'twenty' }
  let!(:score_cert_eighteen) { create :certification, :scorekeeper }
  let!(:assistant_test_eighteen) do
    create :test, level: 'assistant', active: true, certification: assistant_cert_eighteen, new_language_id: 1
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
  let!(:score_test_eighteen) do
    create :test, level: 'scorekeeper', active: true, certification: score_cert_eighteen
  end

  subject { described_class.new(user).perform }

  it 'returns the assistant tests for all versions when ref has no certs' do
    expect(subject.pluck(:id)).to include(
      assistant_test_eighteen.id,
      assistant_test_twenty.id,
      score_test_eighteen.id
    )
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
      expect(subject.pluck(:id)).to include(head_test_eighteen.id)
      expect(subject.pluck(:id)).to include(assistant_test_twenty.id)
      expect(subject.pluck(:id).length).to eq 3 # with scorekeeper
    end
  end

  context 'with a test attempt within the cool down period' do
    let!(:test_attempt) { create :test_attempt, test_level: 'assistant', test: assistant_test_eighteen, referee: user }

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
    let!(:scorekeeper_eighteen) { create :referee_certification, referee: user, certification: score_cert_eighteen }
    let!(:head_eighteen) { create :referee_certification, referee: user, certification: head_cert_eighteen }

    it 'only returns the assistant twenty test' do
      expect(subject.pluck(:id)).to include(assistant_test_twenty.id)
      expect(subject.pluck(:id).length).to eq 1
    end
  end

  context 'with test_attempts but no certifications' do
    let!(:test_attempts) do
      create(
        :test_attempt,
        test_level: 'assistant',
        test: assistant_test_eighteen,
        referee: user,
        created_at: 2.weeks.ago,
        next_attempt_at: 1.week.ago
      )
    end

    it 'returns the assistant eighteen test' do
      expect(subject.pluck(:id)).to include(assistant_test_eighteen.id)
    end

    context 'with multiple test_attempts' do
      let!(:recent_attempt) do
        create :test_attempt, test_level: 'assistant', test: assistant_test_eighteen, referee: user
      end

      it 'does not return the eighteen test' do
        expect(subject.pluck(:id)).to_not include(assistant_test_eighteen.id)
      end
    end
  end

  context 'with recertification' do
    let!(:recert_assistant_twenty) do
      create :test, level: 'assistant', active: true, certification: assistant_cert_twenty, recertification: true
    end
    let!(:recert_assistant_twentytwo) do
      create :test, level: 'assistant', active: true, certification: assistant_cert_twentytwo, recertification: true
    end

    it 'does not return the assistant recertification when ref has no certifications' do
      expect(subject.pluck(:id)).to include(
        assistant_test_eighteen.id,
        assistant_test_twenty.id,
        score_test_eighteen.id
      )
      expect(subject.pluck(:id)).to_not include(recert_assistant_twenty.id)
    end

    context 'with an assistant certification for eighteen' do
      let!(:test_attempt) do
        create(:test_attempt,
               test_level: 'assistant',
               test: assistant_test_eighteen,
               referee: user,
               created_at: 10.days.ago)
      end
      let!(:assistant_eighteen) { create :referee_certification, referee: user, certification: assistant_cert_eighteen }

      before { allow_any_instance_of(TestAttempt).to receive(:in_cool_down_period?).and_return(false) }

      it 'returns the snitch eighteen and initial assistant twenty' do
        expect(subject.pluck(:id).length).to eq 3
        expect(subject.pluck(:id)).to include(assistant_test_twenty.id, snitch_test_eighteen.id)
      end
    end

    context 'with an assistant certification for twenty' do
      let!(:test_attempt) do
        create(:test_attempt,
               test_level: 'assistant',
               test: assistant_test_twenty,
               referee: user,
               created_at: 10.days.ago)
      end
      let!(:assistant_twenty) { create :referee_certification, referee: user, certification: assistant_cert_twenty }

      before { allow_any_instance_of(TestAttempt).to receive(:in_cool_down_period?).and_return(false) }

      it 'returns the snitch eighteen and recert twentytwo' do
        expect(subject.pluck(:id).length).to eq 3
        expect(subject.pluck(:id)).to include(recert_assistant_twentytwo.id)
        expect(subject.pluck(:id)).to include(snitch_test_twenty.id)
      end
    end

  end

  context 'when a user has selected a language' do
    let!(:language) { create :language }
    let!(:user) { create :user, language: language }
    let!(:lang_assistant_eighteen) do
      create(
        :test,
        level: 'assistant',
        active: true,
        certification: assistant_cert_eighteen,
        new_language_id: language.id
      )
    end
    let!(:lang_assistant_twenty) do
      create(
        :test,
        level: 'assistant',
        active: true,
        certification: assistant_cert_twenty,
        new_language_id: language.id
      )
    end

    it "returns only the user's language tests" do
      expect(subject.pluck(:id)).to include(lang_assistant_eighteen.id, lang_assistant_twenty.id)
    end

    context 'when user language is not associated with any test' do
      let!(:other_language) { create :language }
      let!(:user) { create :user, language: other_language }

      it 'returns all languages' do
        expect(subject.pluck(:id).length).to eq 5
        expect(subject.pluck(:id)).to include(
          assistant_test_eighteen.id,
          assistant_test_twenty.id,
          lang_assistant_eighteen.id,
          lang_assistant_twenty.id
        )
      end
    end

    context 'when user language is nil' do
      let!(:user) { create :user, language_id: nil }

      it 'returns all languages' do
        expect(subject.pluck(:id).length).to eq 5
        expect(subject.pluck(:id)).to include(
          assistant_test_eighteen.id,
          assistant_test_twenty.id,
          lang_assistant_eighteen.id,
          lang_assistant_twenty.id
        )
      end
    end
  end
end
