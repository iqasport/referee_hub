require 'csv'

module Services
  class NgbCsvImport
    attr_reader :file_path
    attr_accessor :ngbs

    def initialize(file_path)
      @file_path = file_path
      @ngbs = []
    end

    def perform
      CSV.foreach(file_path, headers: true) do |row|
        row_data = row.to_h.with_indifferent_access

        ngb = find_or_initialize_ngb(row_data)
        next unless ngb&.valid?

        ngb.social_accounts = find_or_initialize_social_accounts(row_data)
        ngbs << ngb
      end

      NationalGoverningBody.import(
        ngbs,
        recursive: true,
        on_duplicate_key_update: %i[acronym country player_count region website]
      )
    end

    private

    def find_or_initialize_ngb(row_data)
      ngb = NationalGoverningBody.find_or_initialize_by(name: row_data.dig(:name))
      region = row_data.dig(:region)
      return unless NationalGoverningBody.regions.key?(region)

      ngb.assign_attributes(
        acronym: row_data.dig(:acronym),
        country: row_data.dig(:country),
        player_count: row_data.dig(:player_count),
        region: region,
        website: row_data.dig(:website)
      )

      ngb
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
