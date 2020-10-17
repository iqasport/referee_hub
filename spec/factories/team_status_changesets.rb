# == Schema Information
#
# Table name: team_status_changesets
#
#  id              :bigint           not null, primary key
#  new_status      :string
#  previous_status :string
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  team_id         :bigint
#
# Indexes
#
#  index_team_status_changesets_on_team_id  (team_id)
#
# Foreign Keys
#
#  fk_rails_...  (team_id => teams.id)
#

FactoryBot.define do
  factory :team_status_changeset do
    team { create :team }
    previous_status { 'inactive' }
    new_status { 'developing' }
  end
end
