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
#  index_referee_certs_on_ref_id_and_cert_id  (referee_id,certification_id) UNIQUE WHERE (revoked_at IS NULL)
#

FactoryBot.define do
  factory :referee_certification do
    referee { create :user }
    certification { create :certification }
    received_at { DateTime.now.utc }
    revoked_at nil
    renewed_at nil

    trait :snitch do
      certification { create :certification, :snitch }
    end

    trait :head do
      certification { create :certification, :head }
    end
  end
end
