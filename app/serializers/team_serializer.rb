# == Schema Information
#
# Table name: teams
#
#  id                         :bigint(8)        not null, primary key
#  city                       :string           not null
#  country                    :string           not null
#  group_affiliation          :integer          default("university")
#  name                       :string           not null
#  state                      :string
#  status                     :integer          default("competitive")
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

class TeamSerializer < BaseSerializer
  attributes :city, :country, :group_affiliation, :name, :state, :status

  belongs_to :national_governing_body, serializer: :national_governing_body
  has_many :social_accounts, serializer: :social_account
end
