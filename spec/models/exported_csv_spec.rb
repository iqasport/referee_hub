# == Schema Information
#
# Table name: exported_csvs
#
#  id             :bigint(8)        not null, primary key
#  export_options :json             not null
#  processed_at   :datetime
#  sent_at        :datetime
#  type           :string
#  url            :string
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  user_id        :integer          not null
#
# Indexes
#
#  index_exported_csvs_on_user_id  (user_id)
#

require 'rails_helper'

RSpec.describe ExportedCsv, type: :model do
  include ActiveJob::TestHelper

  let(:user) { create :user }
  let(:csv) { user.exported_csvs.last }

  before { Fog.mock! }

  subject { ExportedCsv::TeamExport.create!(user: user) }

  it 'processes the csv' do
    subject

    expect(csv.url).to_not be_nil
    expect(csv.processed_at).to_not be_nil
  end

  it 'enqueues a result email' do
    ActiveJob::Base.queue_adapter = :test
    expect { subject }.to have_enqueued_job.on_queue('mailers')
  end

  it 'delivers the email' do
    expect { perform_enqueued_jobs { subject } }.to change { ActionMailer::Base.deliveries.size }.by(1)
  end

  it 'sends to the correct user' do
    perform_enqueued_jobs do
      subject
    end

    sent_mail = ActionMailer::Base.deliveries.last
    expect(sent_mail.to[0]).to eq user.email
  end
end
