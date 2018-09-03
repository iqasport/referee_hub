class CreateRefereeLocations < ActiveRecord::Migration[5.2]
  def change
    create_table :referee_locations do |t|
      t.integer :referee_id, null: false
      t.integer :national_governing_body_id, null: false

      t.timestamps
    end

    add_index :referee_locations,
              %i[referee_id national_governing_body_id],
              unique: true,
              name: 'index_referee_locations_on_referee_id_and_ngb_id'
  end
end
