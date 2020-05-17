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

require 'rails_helper'

RSpec.describe Team, type: :model do
  let(:team) { create :team }

  subject { team.update!(status: 'developing') }

  it 'generates a changeset' do
    expect { subject }.to change { team.reload.team_status_changesets.count }.by(1)
  end

  context 'when updating a different attribute' do
    subject { team.update!(name: 'new name') }

    it 'does not generate a changeset' do
      expect { subject }.to_not change { team.reload.team_status_changesets.count }
    end
  end
end
