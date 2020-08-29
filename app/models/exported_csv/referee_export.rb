# == Schema Information
#
# Table name: exported_csvs
#
#  id             :bigint           not null, primary key
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

require 'csv'

class ExportedCsv::RefereeExport < ExportedCsv
  COLUMN_HEADERS = %w[Name Teams Certifications].freeze

  def generate_csv_data
    options = filter_options.presence || {}
    referees = Services::FilterReferees.new(options).filter
    referees = referees.respond_to?(:where) ? referees : User.where(id: referees)

    CSV.generate(col_sep: ',', quote_char: '"') do |csv|
      csv << COLUMN_HEADERS

      referees.each do |referee|
        csv << build_referee_row(referee)
      end
    end
  end

  def build_referee_row(referee)
    name = if referee.export_name? && referee.full_name.length > 1
             referee.full_name
           else
             'Anonymous Referee'
           end
    teams = referee.teams.pluck(:name).join(', ')
    certifications = referee.certifications.pluck(:level).join(', ')

    [
      name,
      teams,
      certifications
    ]
  end

  private

  def filter_options
    JSON.parse(export_options)
  end
end
