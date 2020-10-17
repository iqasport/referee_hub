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

FactoryBot.define do
  factory :national_governing_body_stat do
    national_governing_body { create :national_governing_body }
    total_referees_count { 1 }
    head_referees_count { 1 }
    assistant_referees_count { 1 }
    snitch_referees_count { 1 }
  end
end
