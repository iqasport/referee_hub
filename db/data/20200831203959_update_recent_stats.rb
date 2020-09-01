class UpdateRecentStats < ActiveRecord::Migration[6.0]
  def up
    ngbs_with_teams = NationalGoverningBody.joins(:teams).uniq.pluck(:id)
    empty_stats = NationalGoverningBodyStat.where(national_governing_body_id: ngbs_with_teams, total_teams_count: 0)

    empty_stats.each { |stat| stat.update_team_counts }
  end

  def down
    raise ActiveRecord::IrreversibleMigration
  end
end
