# == Schema Information
#
# Table name: national_governing_bodies
#
#  id           :bigint(8)        not null, primary key
#  acronym      :string
#  country      :string
#  image_url    :string
#  name         :string           not null
#  player_count :integer          default(0), not null
#  region       :integer
#  website      :string
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_national_governing_bodies_on_region  (region)
#

class NationalGoverningBodySerializer < BaseSerializer
  attributes :name, :website
end
