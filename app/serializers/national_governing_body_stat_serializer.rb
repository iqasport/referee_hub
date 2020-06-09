# == Schema Information
#
# Table name: national_governing_body_stats
#
#  id                         :bigint(8)        not null, primary key
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
#  national_governing_body_id :bigint(8)
#
# Indexes
#
#  ngb_stats_on_ngb_id  (national_governing_body_id)
#
# Foreign Keys
#
#  fk_rails_...  (national_governing_body_id => national_governing_bodies.id)
#

class NationalGoverningBodyStatSerializer < BaseSerializer
  attributes :assistant_referees_count, 
             :community_teams_count,
             :competitive_teams_count,
             :developing_teams_count,
             :head_referees_count,
             :inactive_teams_count,
             :snitch_referees_count,
             :team_status_change_count,
             :total_referees_count,
             :total_teams_count,
             :uncertified_count,
             :university_teams_count,
             :youth_teams_count,
             :start,
             :end_time
end
