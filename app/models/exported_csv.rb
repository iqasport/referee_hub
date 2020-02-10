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

class ExportedCsv < ApplicationRecord
  belongs_to :user

  after_commit :process_csv, on: :create, unless: :processed_at

  after_save :mail_csv, if: proc { |csv| csv.saved_change_to_processed_at? && !csv.sent_at? }

  def self.format_type(type_string)
    type_string.split('::')[1].titleize
  end

  def process_csv
    upload(generate_csv_data)

    self.processed_at = Time.zone.now
    save
  end

  def generate_csv_data; end

  def upload(csv_data)
    current_time = Time.now.utc
    iso_time = format('%10.5f', current_time.to_f).to_i
    type_folder = ExportedCsv.format_type(type).parameterize(separator: '_')
    key = "exports/#{type_folder}/#{iso_time}.csv"

    url = Services::S3::Uploader.new(
      key: key,
      data: csv_data,
      content_type: 'text/csv',
      extension: 'csv',
      public_access: false
    ).perform

    self.url = url
  end

  def mail_csv
    UserMailer.with(user: user, csv: self).export_csv_email.deliver_later
  end
end
