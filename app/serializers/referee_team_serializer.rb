# == Schema Information
#
# Table name: referee_teams
#
#  id               :bigint(8)        not null, primary key
#  association_type :integer          default("player")
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  referee_id       :bigint(8)
#  team_id          :bigint(8)
#
# Indexes
#
#  index_referee_teams_on_referee_id                       (referee_id)
#  index_referee_teams_on_referee_id_and_association_type  (referee_id,association_type) UNIQUE
#  index_referee_teams_on_team_id                          (team_id)
#
# Foreign Keys
#
#  fk_rails_...  (referee_id => users.id)
#  fk_rails_...  (team_id => teams.id)
#

class RefereeTeamSerializer < BaseSerializer
  attributes :association_type, :referee_id, :team_id
end