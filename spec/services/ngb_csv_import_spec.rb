require 'csv'
require 'rails_helper'

describe Services::NgbCsvImport do
  let(:headers) { 'name,acronym,country,player_count,region,membership_status,website,url_1' }
  let(:row1) { 'US Quidditch,USQ,United States,2000,north_america,full,www.usquidditch.com,www.instagram.com/usquidditch' }
  let(:row2) { 'Quidditch UK,QUK,Great Britain,1000,europe,full,www.quidditchuk.com,www.facebook.com/quidditchuk' }
  let(:row3) { 'Quidditch Australia,QA,Australia,1000,asia,full,www.quidditchaustralia.com,www.twitter.com/quidditchaus' }
  let(:rows) { [headers, row1, row2, row3] }
  let(:file_path) { 'tmp/ngb_csv_import_test.csv' }
  let(:mapped_headers) do
    {
      'name': 'name',
      'acronym': 'acronym',
      'country': 'country',
      'player_count': 'player_count',
      'region': 'region',
      'membership_status': 'membership_status',
      'website': 'website',
      'url_1': 'url_1',
    }
  end
  let!(:csv) do
    CSV.open(file_path, 'w') do |csv|
      rows.each { |row| csv << row.split(',') }
    end
  end

  after(:each) { File.delete(file_path) }

  subject { described_class.new(file_path, mapped_headers.to_json).perform }

  it 'creates 3 ngbs' do
    expect { subject }.to change { NationalGoverningBody.count }.by 3
  end

  it 'creates social media accounts' do
    subject

    expect(NationalGoverningBody.first.social_accounts.count).to be 1
    expect(NationalGoverningBody.first.social_accounts.first.account_type).to eq 'instagram'
    expect(NationalGoverningBody.first.social_accounts.first.url).to eq 'www.instagram.com/usquidditch'
  end

  context 'when the team already exists' do
    let!(:ngb) { create :national_governing_body, name: 'US Quidditch' }

    it 'updates the ngb' do
      subject

      expect(ngb.reload.acronym).to eq 'USQ'
      expect(ngb.reload.country).to eq 'United States'
      expect(ngb.reload.player_count).to eq 2000
      expect(ngb.reload.region).to eq 'north_america'
      expect(ngb.reload.website).to eq 'www.usquidditch.com'
    end

    it 'adds the social account' do
      expect { subject }.to change { ngb.social_accounts.count }.by(1)
    end
  end

  context 'when the team information is invalid' do
    let(:row1) { 'US Quidditch,USQ,United States,2000,nonsense,morenonsense,www.usquidditch.com,www.instagram.com/usquidditch' }

    it 'does not create that ngb' do
      expect { subject }.to change { NationalGoverningBody.count }.by 2
    end
  end
end
