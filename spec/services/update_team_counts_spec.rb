require 'rails_helper'

describe Services::UpdateTeamCounts do
  let!(:ngb) { create :national_governing_body }
  let!(:teams) { create_list :team, 5, joined_at: 3.days.ago, national_governing_body: ngb }
  let!(:stat) { create :national_governing_body_stat, end_time: 1.day.ago, national_governing_body: ngb }

  subject { described_class.new(stat).perform }

  it 'updates the stat' do
    expect { subject }.to change { stat.total_teams_count }.from(0).to(teams.length)
  end
end
