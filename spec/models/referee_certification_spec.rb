# == Schema Information
#
# Table name: referee_certifications
#
#  id               :bigint           not null, primary key
#  needs_renewal_at :datetime
#  received_at      :datetime
#  renewed_at       :datetime
#  revoked_at       :datetime
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  certification_id :integer          not null
#  referee_id       :integer          not null
#
# Indexes
#
#  index_referee_certs_on_ref_id_and_cert_id  (referee_id,certification_id) UNIQUE WHERE (revoked_at IS NULL)
#

require 'rails_helper'

RSpec.describe RefereeCertification, type: :model do
  let(:referee) { create :user }
  let(:certification) { create :certification }
  let(:ref_cert) { build :referee_certification, referee: referee, certification: certification }

  subject { ref_cert.save }

  it 'is a valid certificiation' do
    expect(ref_cert).to be_valid
    expect { subject }.to_not raise_error
  end

  context 'when a certification of the same level already exists' do
    before { create :referee_certification, referee: referee, certification: certification }

    it 'raises an error' do
      expect { subject }.to raise_error(ActiveRecord::RecordNotUnique)
    end
  end

  context 'with already existing certifications' do
    let(:snitch) { create :certification, :snitch }
    let(:assistant) { create :certification }
    let(:head) { create :certification, :head }
    let(:score) { create :certification, :scorekeeper }
    let(:field) { create :certification, :field }

    before { referee.certifications << [assistant, snitch, score] }

    context 'with the required certifications' do
      let(:ref_cert) { build :referee_certification, referee: referee, certification: head }

      it 'is a valid certification' do
        expect(ref_cert).to be_valid
        expect { subject }.to_not raise_error
      end
    end

    # Temp remove test as HRs aren't able to sit their tests
    # context 'without the required certifications' do
    #   let(:ref_cert) { build :referee_certification, referee: referee, certification: field }

    #   it 'is not a valid certification' do
    #     subject
    #     expect(ref_cert.errors.messages[:certification][0]).to eq described_class::FIELD_CERTIFICATION_ERROR
    #   end
    # end
  end

  context 'when the certification is revoked' do
    before { ref_cert.update(revoked_at: DateTime.now.utc) }

    subject { referee.certifications }

    it 'does not appear in the default scope' do
      expect(subject).to be_empty
    end
  end

  context 'with certifications in different version' do
    let(:new_cert) { create :certification, version: 'twenty' }
    let(:ref_cert) { build :referee_certification, referee: referee, certification: new_cert }
    let!(:old_cert_diff_version) { create :referee_certification, referee: referee, certification: certification }

    it 'is a valid certificiation' do
      expect(ref_cert).to be_valid
      expect { subject }.to_not raise_error
    end

    context 'without a prerequisite in the same version' do
      let(:new_cert) { create :certification, :snitch, version: 'twenty' }

      it 'is not a valid certification' do
        subject
        expect(ref_cert.errors.messages[:certification][0]).to eq described_class::SNITCH_CERTIFICATION_ERROR
      end
    end
  end
end
