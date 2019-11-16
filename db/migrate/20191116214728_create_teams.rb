class CreateTeams < ActiveRecord::Migration[5.2]
  def change
    create_table :teams do |t|
      t.string :name, null: false
      t.string :city, null: false
      t.string :state
      t.string :country, null: false
      t.integer :status, default: 0
      t.integer :group_affiliation, default: 0
      t.references :national_governing_body, foreign_key: true

      t.timestamps
    end
  end
end
