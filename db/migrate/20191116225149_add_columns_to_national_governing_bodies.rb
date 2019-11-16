class AddColumnsToNationalGoverningBodies < ActiveRecord::Migration[5.2]
  def change
    change_table :national_governing_bodies do |t|
      t.integer :player_count, default: 0, null: false
      t.string :image_url
      t.string :country
      t.string :acronym
    end
  end
end
