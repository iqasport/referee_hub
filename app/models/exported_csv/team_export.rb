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

require 'csv'

class ExportedCsv::TeamExport < ExportedCsv
  COLUMN_HEADERS = [
    'Name',
    'National Governing Body',
    'City',
    'State/Provence',
    'Status',
    'Group Type'
  ].freeze

  def generate_csv_data
    teams = Services::FilterTeams.new(export_options.to_h).perform
    teams = teams.respond_to?(:where) ? teams : Team.where(id: teams)

    CSV.generate(col_sep: ',', quote_char: '"') do |csv|
      csv << COLUMN_HEADERS

      teams.each do |team|
        csv << build_team_row(team)
      end
    end
  end

  def build_team_row(team)
    [
      team.name,
      team.national_governing_body.name,
      team.city,
      team.state,
      team.status,
      team.group_affiliation
    ]
  end
end
