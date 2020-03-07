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
    case export_options.class
    when Hash
      export_options
    when String
      JSON.parse(export_options)
    end
  end
end
