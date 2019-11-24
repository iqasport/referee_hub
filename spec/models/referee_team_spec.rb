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
#  fk_rails_...  (referee_id => referees.id)
#  fk_rails_...  (team_id => teams.id)
#

require 'rails_helper'

RSpec.describe RefereeTeam, type: :model do
  let(:referee_team) { build :referee_team }

  subject { referee_team.valid? }

  it { expect(subject).to be_truthy }

  context 'when a referee already has a team' do
    let(:ngb) { create :national_governing_body }
    let(:referee) { create :user, :referee }
    let(:team_1) { create :team, national_governing_body: ngb }
    let(:team_2) { create :team, national_governing_body: ngb }
    let!(:existing_referee_team) { create :referee_team, team: team_1, referee: referee, association_type: 'player' }
    let(:referee_team) { build :referee_team, team: team_2, referee: referee, association_type: 'coach' }

    it { expect(subject).to be_truthy }

    context 'with an already existing association type' do
      let(:referee_team) { build :referee_team, team: team_2, referee: referee, association_type: 'player' }

      it { expect(subject).to be_falsey }
    end
  end
end
