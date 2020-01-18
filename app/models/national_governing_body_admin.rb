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

class NationalGoverningBodyAdmin < ApplicationRecord
  belongs_to :user
  belongs_to :national_governing_body

  validates :user_id, presence: true, uniqueness: true
  validates :national_governing_body_id, presence: true

  after_create :ensure_admin_role

  def ensure_admin_role
    return if user.ngb_admin?

    Role.create!(user_id: user.id, access_type: 'ngb_admin')
  end
end
