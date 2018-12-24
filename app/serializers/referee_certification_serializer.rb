# == Schema Information
#
# Table name: referee_certifications
#
#  id               :bigint(8)        not null, primary key
#  needs_renewal_at :datetime
#  received_at      :datetime
#  renewed_at       :datetime
#  revoked_at       :datetime
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  certification_id :integer          not null
#  referee_id       :integer          not null
#
# Indexes
#
#  index_referee_certifications_on_referee_id_and_certification_id  (referee_id,certification_id) UNIQUE WHERE (revoked_at IS NULL)
#

class RefereeCertificationSerializer
  include FastJsonapi::ObjectSerializer

  attributes :needs_renewal_at,
             :received_at,
             :renewed_at,
             :revoked_at,
             :referee_id,
             :certification_id

  attribute :level do |ref_cert, _params|
    cert = Certification.find_by id: ref_cert.certification_id
    cert.level
  end
end
