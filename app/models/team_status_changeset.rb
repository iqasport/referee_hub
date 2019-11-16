# == Schema Information
#
# Table name: team_status_changesets
#
#  id              :bigint(8)        not null, primary key
#  new_status      :string
#  previous_status :string
#  created_at      :datetime         not null
#  updated_at      :datetime         not null
#  team_id         :bigint(8)
#
# Indexes
#
#  index_team_status_changesets_on_team_id  (team_id)
#
# Foreign Keys
#
#  fk_rails_...  (team_id => teams.id)
#

class TeamStatusChangeset < ApplicationRecord
  belongs_to :team
end
