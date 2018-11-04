# == Schema Information
#
# Table name: national_governing_bodies
#
#  id         :bigint(8)        not null, primary key
#  name       :string           not null
#  website    :string
#  created_at :datetime         not null
#  updated_at :datetime         not null
#

# Data model for NationalGoverningBodies, currently only used for Referees to assign for themselves and users to search
# based this association
class NationalGoverningBody < ApplicationRecord
  has_many :referee_locations, dependent: :destroy
  has_many :referees, through: :referee_locations
  has_many :certified_referees, -> { certified }, through: :referee_locations, source: :referee
end
