require 'csv'
require 'rails_helper'

describe Services::TeamCsvImport do
  let(:ngb) { create :national_governing_body }
  let(:headers) { 'name,city,country,state,age_group,status,joined_at,url_1' }
  let(:row1) { 'DCQC,Washington,USA,DC,community,competitive,01-01-2018,www.usquidditch.com/dcqc' }
  let(:row2) { 'Bowling Green Quidditch,Bowling Green,USA,OH,university,developing,02-01-2018,www.facebook.com/bgsu' }
  let(:row3) { 'Vassar Quidditch,Poughkeepsie,USA,NY,youth,inactive,03-01-2018,www.twitter.com/vassar' }
  let(:rows) { [headers, row1, row2, row3] }
  let(:file_path) { 'tmp/team_csv_import_test.csv' }
  let(:mapped_headers) do
    {
      'name': 'name',
      'city': 'city',
      'country': 'country',
      'state': 'state',
      'age_group': 'age_group',
      'status': 'status',
      'joined_at': 'joined_at',
      'url_1': 'url_1'
    }
  end
  let!(:csv) do
    CSV.open(file_path, 'w') do |csv|
      rows.each { |row| csv << row.split(',') }
    end
  end
  let(:expected_datetime) { Date.strptime('01-01-2018', '%d-%m-%Y').to_datetime }

  after(:each) { File.delete(file_path) }

  subject { described_class.new(file_path, ngb, mapped_headers.to_json).perform }

  it 'returns the imported team ids' do
    expect(subject.length).to eq 3
  end

  it 'creates 3 teams under the provided ngb' do
    expect { subject }.to change { ngb.teams.count }.by 3
  end

  it 'creates social media accounts' do
    subject

    expect(ngb.teams.first.social_accounts.count).to be 1
    expect(ngb.teams.first.social_accounts.first.account_type).to eq 'other'
    expect(ngb.teams.first.social_accounts.first.url).to eq 'www.usquidditch.com/dcqc'
  end

  it 'correctly parses the joined date' do
    subject

    expect(ngb.teams.first.joined_at).to eq expected_datetime
  end

  context 'when the team already exists' do
    let!(:team) { create :team, national_governing_body: ngb, name: 'DCQC' }

    it 'updates the team' do
      subject

      expect(team.reload.city).to eq 'Washington'
      expect(team.reload.country).to eq 'USA'
      expect(team.reload.state).to eq 'DC'
      expect(team.reload.group_affiliation).to eq 'community'
      expect(team.reload.status).to eq 'competitive'
    end

    it 'adds the social account' do
      expect { subject }.to change { team.social_accounts.count }.by(1)
    end
  end

  context 'when the team information is unknown' do
    let(:row1) { 'DCQC,Washington,USA,DC,nothing,nonsense,01-01-2018,www.usquidditch.com/dcqc' }
    let(:team) { ngb.teams.find_by(name: 'DCQC') }

    it 'creates the team with default enums' do
      subject

      expect(team.status).to eq 'other'
      expect(team.group_affiliation).to eq 'not_applicable'
    end
  end
end
