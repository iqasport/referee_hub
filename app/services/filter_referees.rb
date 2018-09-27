module Services
  class FilterReferees
    attr_accessor :search_query, :filter_by, :relation

    def initialize(params)
      @search_query = params.delete(:q)
      @filter_by = params.delete(:filter_by)
      @relation = Referee.all
    end

    def filter
      return relation if filter_by.blank? && search_query.blank?

      @relation = search_by_name if search_query.present?

      if filter_by.present?
        @relation = filter_by_certification if filter_by[:certifications].present?
        @relation = filter_by_national_governing_body if filter_by[:national_governing_bodies].present?
      end

      relation || []
    end

    private

    def search_by_name
      relation.where("coalesce(first_name, '') || ' ' || coalesce(last_name, '') ilike '%' || ? || '%'", search_query)
    end

    def filter_by_certification
      certification_levels = filter_by.fetch(:certifications)
      return relation if certification_levels.blank?

      relation.joins(:certifications).where(certifications: { level: certification_levels })
    end

    def filter_by_national_governing_body
      national_governing_body_ids = filter_by.fetch(:national_governing_bodies)
      return relation if national_governing_body_ids.blank?

      relation
        .joins(:national_governing_bodies)
        .where(national_governing_bodies: { id: national_governing_body_ids })
    end
  end
end
