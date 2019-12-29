require 'csv'

module Services
  class TeamCsvImport
    attr_reader :file_path, :ngb
    attr_accessor :teams

    NGBMissingError = Class.new(StandardError)

    def initialize(file_path, ngb)
      @file_path = file_path
      @ngb = ngb
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

      Team.import teams, recursive: true, on_duplicate_key_update: %i[city country state group_affiliation status]
    end

    private

    def find_or_initialize_team(row_data)
      team = Team.where(national_governing_body_id: ngb.id).find_or_initialize_by(name: row_data.dig(:name))
      status = row_data.dig(:status)
      group_affiliation = row_data.dig(:age_group)
      return unless Team.statuses.key?(status)
      return unless Team.group_affiliations.key?(group_affiliation)

      team.assign_attributes(
        city: row_data.dig(:city),
        country: row_data.dig(:country),
        group_affiliation: group_affiliation,
        name: row_data.dig(:name),
        state: row_data.dig(:state),
        status: status
      )

      team
    end

    def find_or_initialize_social_accounts(row_data)
      social_accounts = []
      all_keys = row_data.keys
      url_keys = all_keys.select { |key| key =~ /url_\d+$/ }

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
