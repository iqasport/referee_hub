# == Schema Information
#
# Table name: teams
#
#  id                         :bigint(8)        not null, primary key
#  city                       :string           not null
#  country                    :string           not null
#  name                       :string           not null
#  state                      :string
#  status                     :integer          default(0)
#  type                       :integer          default(0)
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint(8)        not null
#
# Indexes
#
#  index_teams_on_national_governing_body_id  (national_governing_body_id)
#

FactoryBot.define do
  factory :team do
    name 'DC Quiddicth Club'
    city 'Washington'
    state 'DC'
    country 'United States'
    status 1
    type 1
  end
end
