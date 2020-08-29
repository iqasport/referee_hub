module Services
  class FilterTeams
    attr_accessor :search_query, :national_governing_bodies, :status, :group_affiliation, :relation, :query_hash

    def initialize(params = {})
      sanitized_params = params.to_h.with_indifferent_access
      @search_query = sanitized_params.delete(:q)
      @national_governing_bodies = sanitized_params.delete(:national_governing_bodies)
      @status = sanitized_params.delete(:status)
      @group_affiliation = sanitized_params.delete(:group_affiliation)
      @relation = Team.all
      @query_hash = {
        search: search_query,
        national_governing_bodies: national_governing_bodies,
        status: status,
        group_affiliation: group_affiliation
      }
    end

    def filter
      return relation if query_hash.values.blank?

      @relation = filter_by_national_governing_body if national_governing_bodies.present?
      @relation = search_by_name if search_query.present?
      @relation = filter_by_status if status.present?
      @relation = filter_by_group_affiliation if group_affiliation.present?

      relation.pluck(:id)
    end

    private

    def filter_by_national_governing_body
      return relation if national_governing_bodies.blank?

      relation.where(national_governing_body_id: national_governing_bodies)
    end

    def search_by_name
      relation.where("name ilike '%' || ? || '%'", search_query)
    end

    def filter_by_status
      relation.where(status: status)
    end

    def filter_by_group_affiliation
      relation.where(group_affiliation: group_affiliation)
    end
  end
end
