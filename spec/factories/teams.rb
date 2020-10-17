# == Schema Information
#
# Table name: teams
#
#  id                         :bigint           not null, primary key
#  city                       :string           not null
#  country                    :string           not null
#  group_affiliation          :integer          default("university")
#  joined_at                  :datetime
#  name                       :string           not null
#  state                      :string
#  status                     :integer          default("competitive")
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint
#
# Indexes
#
#  index_teams_on_national_governing_body_id  (national_governing_body_id)
#
# Foreign Keys
#
#  fk_rails_...  (national_governing_body_id => national_governing_bodies.id)
#

FactoryBot.define do
  factory :team do
    name { FFaker::AddressUS.city }
    city { FFaker::AddressUS.city }
    state { FFaker::AddressUS.state }
    country { FFaker::AddressUS.country }
    status { 0 }
    group_affiliation { 0 }
    national_governing_body { create :national_governing_body }
  end
end
