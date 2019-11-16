class CreateNationalGoverningBodyStats < ActiveRecord::Migration[5.2]
  def change
    create_table :national_governing_body_stats do |t|
      t.references :national_governing_body, foreign_key: true, index: { name: 'ngb_stats_on_ngb_id' }
      t.integer :total_referees_count, default: 0
      t.integer :head_referees_count, default: 0
      t.integer :assistant_referees_count, default: 0
      t.integer :snitch_referees_count, default: 0
      t.integer :competitive_teams_count, default: 0
      t.integer :developing_teams_count, default: 0
      t.integer :inactive_teams_count, default: 0
      t.integer :youth_teams_count, default: 0
      t.integer :university_teams_count, default: 0
      t.integer :community_teams_count, default: 0
      t.integer :team_status_change_count, default: 0

      t.timestamps
    end
  end
end
