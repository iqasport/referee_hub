class CreateTeamStatusChangesets < ActiveRecord::Migration[5.2]
  def change
    create_table :team_status_changesets do |t|
      t.references :team, foreign_key: true
      t.string :previous_status
      t.string :new_status

      t.timestamps
    end
  end
end
