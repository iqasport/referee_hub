# == Schema Information
#
# Table name: national_governing_body_stats
#
#  id                         :bigint           not null, primary key
#  assistant_referees_count   :integer          default(0)
#  community_teams_count      :integer          default(0)
#  competitive_teams_count    :integer          default(0)
#  developing_teams_count     :integer          default(0)
#  end_time                   :datetime
#  head_referees_count        :integer          default(0)
#  inactive_teams_count       :integer          default(0)
#  snitch_referees_count      :integer          default(0)
#  start                      :datetime
#  team_status_change_count   :integer          default(0)
#  total_referees_count       :integer          default(0)
#  total_teams_count          :integer          default(0)
#  uncertified_count          :integer          default(0)
#  university_teams_count     :integer          default(0)
#  youth_teams_count          :integer          default(0)
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint
#
# Indexes
#
#  ngb_stats_on_ngb_id  (national_governing_body_id)
#
# Foreign Keys
#
#  fk_rails_...  (national_governing_body_id => national_governing_bodies.id)
#

class NationalGoverningBodyStat < ApplicationRecord
  belongs_to :national_governing_body

  def self.by_month(month)
    where('EXTRACT(month FROM end_time::date)::integer = ?', month)
  end

  def update_team_counts
    Services::UpdateTeamCounts.new(self).perform
  end
end
