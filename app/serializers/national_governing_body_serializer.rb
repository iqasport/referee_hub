# == Schema Information
#
# Table name: national_governing_bodies
#
#  id                :bigint           not null, primary key
#  acronym           :string
#  country           :string
#  image_url         :string
#  membership_status :integer          default("area_of_interest"), not null
#  name              :string           not null
#  player_count      :integer          default(0), not null
#  region            :integer
#  website           :string
#  created_at        :datetime         not null
#  updated_at        :datetime         not null
#
# Indexes
#
#  index_national_governing_bodies_on_region  (region)
#

class NationalGoverningBodySerializer < BaseSerializer
  attributes :name, 
             :website, 
             :acronym, 
             :player_count, 
             :region, 
             :country, 
             :logo_url,
             :membership_status

  has_many :social_accounts
  has_many :teams
  has_many :referees
  has_many :stats, serializer: :national_governing_body_stat
end
