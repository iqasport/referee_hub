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

class RefereeCertificationSerializer
  include FastJsonapi::ObjectSerializer

  attributes :needs_renewal_at,
             :received_at,
             :renewed_at,
             :revoked_at,
             :referee_id,
             :certification_id
end
