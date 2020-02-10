require 'rails_helper'

describe Services::S3::Uploader do
  let(:user) { create :user }
  let(:ngb) { create :national_governing_body }
  let(:teams) { create_list :team, 5, national_governing_body: ngb }
  let(:export_options) { { national_governing_bodies: [ngb.id] } }
  let(:data) { ExportedCsv::TeamExport.new(user: user, export_options: export_options.to_json).generate_csv_data }
  let(:key) { 'team_export.csv' }
  let(:extension) { 'csv' }

  before do
    Fog.mock!
  end

  subject do
    described_class.new(
      data: data,
      content_type: 'text/csv',
      extension: extension,
      public_access: false,
      key: key
    ).perform
  end

  it 'returns the created url' do
    expect(subject).to eq "https://nonsense.s3.amazonaws.com/#{key}"
  end

  context 'with invalid data' do
    let(:data) { nil }

    it { expect { subject }.to raise_error(Services::S3::Uploader::InvalidData) }
  end

  context 'with invalid extension' do
    let(:extension) { nil }

    it { expect { subject }.to raise_error(Services::S3::Uploader::InvalidExtension) }
  end

  context 'with invalid key' do
    let(:key) { nil }

    it { expect { subject }.to raise_error(Services::S3::Uploader::InvalidKey) }
  end
end
