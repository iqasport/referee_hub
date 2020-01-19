require 'rails_helper'

RSpec.describe Services::CreateAndUpdateSocial do
  let!(:owner) { create :team }
  let(:urls) { ['www.facebook.com/dcqc'] }
  let(:action) { :create }

  subject { described_class.new(urls, action, owner).perform }

  it 'initializes a social account' do
    output = subject.first

    expect(output.persisted?).to be_falsey
    expect(output.url).to eq 'www.facebook.com/dcqc'
    expect(output.account_type).to eq 'facebook'
  end

  context 'with update action' do
    let(:action) { :update }

    it 'initializes a social account' do
      output = subject.first

      expect(output.persisted?).to be_falsey
      expect(output.url).to eq 'www.facebook.com/dcqc'
      expect(output.account_type).to eq 'facebook'
    end

    context 'with an already existing social account' do
      let!(:other_social) { create :social_account, ownable_id: owner.id }

      it 'should remove the existing social account' do
        expect { subject }.to change { owner.social_accounts.count }.by(-1)
      end

      context 'when the already existing url is included' do
        let(:urls) { ['www.facebook.com/dcqc', other_social.url] }

        it 'does not instantiate the existing url' do
          output = subject

          expect(output.length).to eq 1
          expect(output.first.url).to eq 'www.facebook.com/dcqc'
          expect(output.first.account_type).to eq 'facebook'
        end
      end
    end
  end
end
