# == Schema Information
#
# Table name: test_results
#
#  id                      :bigint(8)        not null, primary key
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
#  cm_link_result_id       :integer
#  referee_id              :integer          not null
#  test_id                 :integer
#
# Indexes
#
#  index_test_results_on_referee_id  (referee_id)
#

require 'rails_helper'

RSpec.describe TestResult, type: :model do
  let(:referee) { create :referee }
  let(:test_result) { build :test_result, referee: referee, test_level: :assistant }
  let!(:existing_certification) { create :certification }

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
        let(:test_result) { build :test_result, referee: referee, test_level: :assistant }
        let!(:ref_cert) { create :referee_certification, referee: referee, certification: existing_certification }

        it 'updates the existing certification' do
          expect { subject }.to change { ref_cert.reload.renewed_at }.from(nil)
        end
      end
    end

    context 'and passed is false' do
      context 'and a certification already exists' do
        let(:test_result) { build :test_result, :failed, referee: referee, test_level: :assistant }
        let!(:ref_cert) { create :referee_certification, referee: referee, certification: existing_certification }

        it 'updates the existing certification' do
          expect { subject }.to change { ref_cert.reload.revoked_at }.from(nil)
        end
      end
    end
  end
end
