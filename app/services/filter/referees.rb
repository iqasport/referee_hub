module Services
  module Filter
    class Referees
      attr_accessor :search_query, :certifications, :national_governing_bodies, :relation, :query_hash

      def initialize(params = {})
        sanitized_params = params.to_h.with_indifferent_access
        @search_query = sanitized_params.delete(:q)
        @certifications = sanitized_params.delete(:certifications)
        @national_governing_bodies = sanitized_params.delete(:national_governing_bodies)
        @relation = User.includes(:certifications, :roles, :referee_locations, :national_governing_bodies).referee.all
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

        query_hash = {
          referee_locations: {
            national_governing_body_id: national_governing_bodies,
            association_type: 'primary'
          }
        }

        relation.where(query_hash)
      end
    end
  end
end
