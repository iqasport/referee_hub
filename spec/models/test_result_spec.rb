# == Schema Information
#
# Table name: test_results
#
#  id                      :bigint           not null, primary key
#  certificate_url         :string
#  duration                :string
#  minimum_pass_percentage :integer
#  passed                  :boolean
#  percentage              :integer
#  points_available        :integer
#  points_scored           :integer
#  test_level              :integer          default("snitch")
#  time_finished           :time
#  time_started            :time
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  referee_id              :integer          not null
#  test_id                 :integer
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

require 'rails_helper'

RSpec.describe TestResult, type: :model do
  let!(:cert) { create :certification }
  let(:referee) { create :user }
  let!(:test) { create :test, certification_id: cert.id }
  let(:test_result) { build :test_result, referee: referee, test_level: :assistant, test: test }

  subject { test_result.save! }

  context 'when passed has changed' do
    context 'and passed is true' do
      context "when a ref certification doesn't exist" do
        it 'creates a referee certification' do
          expect { subject }.to change { referee.certifications.count }.by(1)
        end

        it 'sets received_at on new creation' do
          subject
          expect(referee.reload.referee_certifications.last.received_at).to_not be_nil
        end

        it 'creates the certification at the appropriate level' do
          subject
          expect(referee.reload.certifications.last.level).to eq 'assistant'
        end
      end

      context 'when a ref certification already exists' do
        let(:test_result) { build :test_result, referee: referee, test_level: :assistant, test: test }
        let!(:ref_cert) { create :referee_certification, referee: referee, certification: cert }

        it 'updates the existing certification' do
          expect { subject }.to change { ref_cert.reload.renewed_at }.from(nil)
        end
      end
    end

    context 'and passed is false' do
      context 'and a certification already exists' do
        let(:test_result) { build :test_result, :failed, referee: referee, test_level: :assistant, test: test }
        let!(:ref_cert) { create :referee_certification, referee: referee, certification: cert }

        it 'updates the existing certification' do
          expect { subject }.to change { ref_cert.reload.revoked_at }.from(nil)
        end
      end
    end
  end

  context 'when test is nil' do
    let(:test) { nil }

    subject { test_result.valid? }

    it { expect(subject).to be_truthy }
  end

  context 'when test is passed and a recertification' do
    let!(:assistant_cert) { create :certification, version: :twentytwo, level: :assistant }
    let!(:snitch_cert) { create :certification, version: :twentytwo, level: :snitch }
    let!(:head_cert) { create :certification, version: :twentytwo, level: :head }

    context 'when level is assistant' do
      let!(:test) { create :test, certification_id: assistant_cert.id, recertification: true, level: 'assistant' }
      it 'awards AR cert' do
        subject
        cert = referee.reload.certifications.last
        expect(cert.level).to eq 'assistant'
        expect(cert.version).to eq 'twentytwo'
      end
    end

    context 'when level is snitch' do
      let!(:test) { create :test, certification_id: snitch_cert.id, recertification: true, level: 'snitch' }
      let(:test_result) { build :test_result, referee: referee, test_level: :snitch, test: test }
      it 'awards AR,SR cert' do
        subject
        certs = referee.reload.certifications.last(2)
        expect(certs[0].level).to eq 'snitch'
        expect(certs[0].version).to eq 'twentytwo'
        expect(certs[1].level).to eq 'assistant'
        expect(certs[1].version).to eq 'twentytwo'
      end
    end

    context 'when level is head' do
      let!(:test) { create :test, certification_id: head_cert.id, recertification: true, level: 'head' }
      let(:test_result) { build :test_result, referee: referee, test_level: :head, test: test }
      it 'awards AR,SR,HR cert' do
        subject
        certs = referee.reload.certifications.last(3)
        expect(certs[0].level).to eq 'head'
        expect(certs[0].version).to eq 'twentytwo'
        expect(certs[1].level).to eq 'assistant'
        expect(certs[1].version).to eq 'twentytwo'
        expect(certs[2].level).to eq 'snitch'
        expect(certs[2].version).to eq 'twentytwo'
      end
    end
  end
end
