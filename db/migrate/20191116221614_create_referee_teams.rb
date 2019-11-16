class CreateRefereeTeams < ActiveRecord::Migration[5.2]
  def change
    create_table :referee_teams do |t|
      t.references :team, foreign_key: true
      t.references :referee, foreign_key: true
      t.integer :association_type, default: 0

      t.timestamps
    end

    add_index :referee_teams, [:referee_id, :association_type], unique: true
  end
end
