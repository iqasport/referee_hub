require 'csv'

module Services
  class TeamCsvImport
    attr_reader :file_path, :ngb, :mapped_headers
    attr_accessor :teams

    NGBMissingError = Class.new(StandardError)

    def initialize(file_path, ngb, mapped_headers)
      @file_path = file_path
      @ngb = ngb
      @mapped_headers = ActiveSupport::JSON.decode(mapped_headers).with_indifferent_access
      @teams = []
    end

    def perform
      raise NGBMissingError, 'NGB must be present to import teams' if ngb.blank?

      CSV.foreach(file_path, headers: true) do |row|
        row_data = row.to_h.with_indifferent_access

        team = find_or_initialize_team(row_data)
        next unless team&.valid?
        team.social_accounts = find_or_initialize_social_accounts(row_data)
        teams << team
      end

      import_results = Team.import(
        teams, 
        recursive: true, 
        on_duplicate_key_update: %i[city country state group_affiliation status], 
        returning: :name
      )
      import_results.ids
    end

    private

    def find_or_initialize_team(row_data)
      team = Team.where(national_governing_body_id: ngb.id).find_or_initialize_by(name: row_data.dig(mapped_headers['name']))
      status = row_data.dig(mapped_headers['status'])
      group_affiliation = row_data.dig(mapped_headers['age_group'])
      return unless Team.statuses.key?(status)
      return unless Team.group_affiliations.key?(group_affiliation)
      # convert mapped_headers['joined_at'] to a datetime. Can only be day-month-year
      joined_at = Date.strptime(row_data.dig(mapped_headers['joined_at']), '%d-%m-%Y').to_datetime
      team.assign_attributes(
        city: row_data.dig(mapped_headers['city']),
        country: row_data.dig(mapped_headers['country']),
        group_affiliation: group_affiliation,
        state: row_data.dig(mapped_headers['state']),
        status: status,
        joined_at: joined_at
      )

      team
    end

    def find_or_initialize_social_accounts(row_data)
      social_accounts = []
      mapped_keys = mapped_headers.keys.select { |key| key =~ /url_\d+$/ }      
      url_keys = mapped_keys.map { |key| mapped_headers[key] }

      url_keys.each do |url_key|
        url = row_data.dig(url_key)
        new_social = SocialAccount.find_or_initialize_by(url: url)
        next if new_social.persisted? # we don't need to update an exisiting record with the same information

        new_social.assign_attributes(account_type: SocialAccount.match_account_type(url))
        social_accounts << new_social
      end

      social_accounts
    end
  end
end
