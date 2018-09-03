class CreateNationalGoverningBodies < ActiveRecord::Migration[5.2]
  def change
    create_table :national_governing_bodies do |t|
      t.string :name, null: false
      t.string :website

      t.timestamps
    end
  end
end
