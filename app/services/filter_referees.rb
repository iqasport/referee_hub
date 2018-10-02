module Services
  class FilterReferees
    attr_accessor :search_query, :certifications, :national_governing_bodies, :relation

    def initialize(params)
      @search_query = params.delete(:q)
      @certifications = params.delete(:certifications)
      @national_governing_bodies = params.delete(:national_governing_bodies)
      @relation = Referee.all
    end

    def filter
      return relation if certifications.blank? && search_query.blank? && national_governing_bodies.blank?

      @relation = search_by_name if search_query.present?
      @relation = filter_by_certification if certifications.present?
      @relation = filter_by_national_governing_body if national_governing_bodies.present?

      relation || []
    end

    private

    def search_by_name
      relation.where("coalesce(first_name, '') || ' ' || coalesce(last_name, '') ilike '%' || ? || '%'", search_query)
    end

    def filter_by_certification
      return relation if certifications.blank?

      relation
        .joins(:certifications)
        .where(certifications: { level: certifications })
        .distinct
    end

    def filter_by_national_governing_body
      return relation if national_governing_bodies.blank?

      relation
        .joins(:national_governing_bodies)
        .where(national_governing_bodies: { id: national_governing_bodies })
        .distinct
    end
  end
end
