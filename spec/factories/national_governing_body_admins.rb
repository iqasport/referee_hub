# == Schema Information
#
# Table name: national_governing_body_admins
#
#  id                         :bigint(8)        not null, primary key
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint(8)        not null
#  user_id                    :bigint(8)        not null
#
# Indexes
#
#  index_national_governing_body_admins_on_ngb_id   (national_governing_body_id)
#  index_national_governing_body_admins_on_user_id  (user_id) UNIQUE
#
# Foreign Keys
#
#  fk_rails_...  (national_governing_body_id => national_governing_bodies.id)
#  fk_rails_...  (user_id => users.id)
#

FactoryBot.define do
  factory :national_governing_body_admin do
    user nil
    national_governing_body nil
  end
end
