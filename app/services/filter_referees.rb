module Services
  class FilterReferees
    attr_accessor :search_query, :certifications, :national_governing_bodies, :relation, :query_hash

    def initialize(params)
      @search_query = params.delete(:q)
      @certifications = params.delete(:certifications)
      @national_governing_bodies = params.delete(:national_governing_bodies)
      @relation = User.includes(:certifications, :national_governing_bodies, :roles).referee.all
      @query_hash = {
        search: search_query,
        certifications: certifications,
        national_governing_bodies: national_governing_bodies
      }
    end

    def filter
      return relation if query_hash.values.blank?

      @relation = search_by_name if search_query.present?
      @relation = filter_by_certification if certifications.present?
      @relation = filter_by_national_governing_body if national_governing_bodies.present?

      relation.pluck(:id)
    end

    private

    def search_by_name
      relation.where("coalesce(first_name, '') || ' ' || coalesce(last_name, '') ilike '%' || ? || '%'", search_query)
    end

    def filter_by_certification
      return relation if certifications.blank?

      relation.where(certifications: { level: certifications })
    end

    def filter_by_national_governing_body
      return relation if national_governing_bodies.blank?

      relation.where(national_governing_bodies: { id: national_governing_bodies })
    end
  end
end
