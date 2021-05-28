module Services
  module Filter
    class SocialAccounts
      attr_accessor :team_ids, :account_ids, :ngb_ids, :relation, :query_hash

      def initialize(params = {})
        sanitized_params = params.to_h.with_indifferent_access
        @team_ids = sanitized_params.delete(:team_ids)
        @account_ids = sanitized_params.delete(:account_ids)
        @ngb_ids = sanitized_params.delete(:ngb_ids)
        @relation = SocialAccount.all
        @query_hash = {
          team_ids: team_ids,
          account_ids: account_ids,
          ngb_ids: ngb_ids
        }
      end

      def filter
        return relation if query_hash.values.blank?

        @relation = filter_by_team_ids if team_ids.present?
        @relation = filter_by_account_ids if account_ids.present?
        @relation = filter_by_ngb_ids f ngb_ids.present?

        relation.pluck(:id)
      end

      def filter_by_team_ids
        relation.where(ownable_type: 'Team', ownable_id: team_ids)
      end

      def filter_by_account_ids
        relation.where(id: account_ids)
      end

      def filter_by_ngb_ids
        relation.where(ownable_type: 'NationalGoverningBody', ownable_id: ngb_ids)
      end
    end
  end
end
