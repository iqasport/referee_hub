require 'rails_helper'

RSpec.describe Services::GenerateNgbStat do
  let(:ngb) { create :national_governing_body }
  let(:ngb_id) { ngb.id }
  let(:assist_refs) { create_list :user, 3, created_at: 3.days.ago }
  let(:snitch_refs) { create_list :user, 3, created_at: 3.days.ago }
  let(:head_refs) { create_list :user, 4, created_at: 3.days.ago }
  let!(:teams) { create_list :team, 10, created_at: 3.days.ago }
  let!(:non_default_teams) { create_list :team, 10, created_at: 3.days.ago, group_affiliation: 'community', status: 'developing' }
  let(:assist_cert) { create :certification }
  let(:snitch_cert) { create :certification, :snitch }
  let(:head_cert) { create :certification, :head }

  before do
    assist_refs.each do |ref| 
      ref.certifications << assist_cert
      RefereeLocation.create(national_governing_body_id: ngb.id, referee_id: ref.id, association_type: 'primary')
    end
    snitch_refs.each do |ref|
      ref.certifications << snitch_cert
      RefereeLocation.create(national_governing_body_id: ngb.id, referee_id: ref.id, association_type: 'primary')
    end
    head_refs.each do |ref|
      ref.certifications << snitch_cert
      ref.certifications << assist_cert
      ref.certifications << head_cert
      RefereeLocation.create(national_governing_body_id: ngb.id, referee_id: ref.id, association_type: 'primary')
    end

  end

  subject { described_class.new(ngb_id, 1.day.ago.to_s).perform }

  it 'creates an ngb stat' do
    expect { subject }.to change { ngb.stats.count }.by(1)
  end

  context 'when a stat in that month already exists' do
    let(:created_time) { DateTime.now }
    let!(:old_stat) { create :national_governing_body_stat, national_governing_body: ngb, end_time: created_time }

    it 'does not create another stat' do
      expect { subject }.to_not change { ngb.stats.count }
    end
  end

  context 'with an invalid ngb' do
    let(:ngb_id) { 1_000_000_000 }

    it 'raises an error' do
      expect { subject }.to raise_error(StandardError)
    end
  end
end
