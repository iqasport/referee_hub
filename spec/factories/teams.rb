# == Schema Information
#
# Table name: teams
#
#  id                         :bigint(8)        not null, primary key
#  city                       :string           not null
#  country                    :string           not null
#  name                       :string           not null
#  state                      :string
#  status                     :integer          default("competitive")
#  type                       :integer          default("university")
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint(8)
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
    name 'DC Quiddicth Club'
    city 'Washington'
    state 'DC'
    country 'United States'
    status 0
    group_affiliation 0
    national_governing_body { create :national_governing_body }
  end
end
