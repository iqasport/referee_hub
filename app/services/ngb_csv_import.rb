require 'csv'

module Services
  class NgbCsvImport
    attr_reader :file_path, :mapped_headers
    attr_accessor :ngbs

    def initialize(file_path, mapped_headers)
      @file_path = file_path
      @ngbs = []
      @mapped_headers = ActiveSupport::JSON.decode(mapped_headers).with_indifferent_access
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
      ngb = NationalGoverningBody.find_or_initialize_by(name: row_data.dig(mapped_headers['name']))
      region = row_data.dig(mapped_headers['region'])
      return unless NationalGoverningBody.regions.key?(region)

      ngb.assign_attributes(
        acronym: row_data.dig(mapped_headers['acronym']),
        country: row_data.dig(mapped_headers['country']),
        player_count: row_data.dig(mapped_headers['player_count']),
        region: region,
        website: row_data.dig(mapped_headers['website'])
      )

      ngb
    end

    def find_or_initialize_social_accounts(row_data)
      social_accounts = []
      mapped_keys = mapped_headers.keys.select { |key| key =~ /url_\d+$/ }
      url_keys = mapped_keys.map { |key| mapped_headers[key] }

      url_keys.each do |url_key|
        url = row_data.dig(url_key)
        new_social = SocialAccount.find_or_initialize_by(url: url)
        next if new_social.persisted? # we don't need to update an existing record with the same information

        new_social.assign_attributes(account_type: SocialAccount.match_account_type(url))
        social_accounts << new_social
      end

      social_accounts
    end
  end
end
