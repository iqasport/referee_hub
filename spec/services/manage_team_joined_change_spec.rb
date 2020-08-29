require 'rails_helper'

describe Services::ManageTeamJoinedChange do
  let!(:ngb) { create :national_governing_body }
  let!(:team) { create :team, national_governing_body: ngb }
  let!(:new_joined_date) { Date.new(2020, 4, 10) }
  let!(:prev_joined_date) { Date.new(2020, 7, 10) }
  let!(:stats) { create_list(:national_governing_body_stat, 8, national_governing_body: ngb) }

  before do
    stats.each_with_index do |stat, index|
      new_time = Date.new(2020, index + 1, 10)
      stat.update(start: new_time.beginning_of_month, end_time: new_time.end_of_month)
    end
  end

  subject { described_class.new(prev_date: prev_joined_date, new_date: new_joined_date, team: team).perform }

  it 'should update +1 the difference between the new date and prev date' do
    expect { subject }.to change {
      ngb.reload.stats.where(end_time: new_joined_date.end_of_month).first.competitive_teams_count
    }.by(1)
  end

  context 'when prev date is older than the new date' do
    let!(:new_joined_date) { Date.new(2020, 7, 10) }
    let!(:prev_joined_date) { Date.new(2020, 4, 10) }

    it 'should update -1 the difference between the prev date and the new date' do
      expect { subject }.to change {
        ngb.reload.stats.where(end_time: prev_joined_date.end_of_month).first.competitive_teams_count
      }.by(-1)
    end
  end

  context 'when new date is outside of the current stats' do
    context 'when newer than the last stat' do
      let!(:new_joined_date) { Date.new(2020, 9, 10) }
      let!(:prev_joined_date) { Date.new(2020, 4, 10) }

      it 'should update -1 the last stat' do
        expect { subject }.to change {
          ngb.reload.stats.order(end_time: :desc).first.competitive_teams_count
        }.by(-1)
      end

      it 'should update -1 the difference between the prev date and the new date' do
        expect { subject }.to change {
          ngb.reload.stats.where(end_time: prev_joined_date.end_of_month).first.competitive_teams_count
        }.by(-1)
      end
    end

    context 'when older than the oldest stat' do
      let!(:new_joined_date) { Date.new(2019, 8, 10) }
      let!(:prev_joined_date) { Date.new(2020, 4, 10) }

      it 'should update +1 the oldest stat' do
        expect { subject }.to change {
          ngb.reload.stats.order(end_time: :desc).last.competitive_teams_count
        }.by(1)
      end
    end
  end

  context "when the team doesn't have a stat value" do
    let(:team) { create :team, group_affiliation: 'not_applicable', status: 'other' }

    it 'should not update the stat' do
      expect { subject }.to_not change {
        ngb.reload.stats.where(end_time: new_joined_date.end_of_month).first.competitive_teams_count
      }
    end
  end
end
