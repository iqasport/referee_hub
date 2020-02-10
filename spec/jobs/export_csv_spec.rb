require 'rails_helper'

RSpec.describe ExportCsvJob, type: :job do
  let(:user) { create :user }
  let(:type) { 'ExportedCsv::TeamExport' }
  let(:export_options) { { national_governing_bodies: [123] }.to_json }

  subject { described_class.perform_later(user: user, type: type, export_options: export_options) }

  it 'should enqueue the job with the correct params' do
    ActiveJob::Base.queue_adapter = :test

    expect { subject }.to have_enqueued_job.exactly(:once)
  end
end
