class CreateTests < ActiveRecord::Migration[5.2]
  def change
    create_table :tests do |t|
      t.integer :level, default: 0
      t.integer :cm_test_id
      t.string :name
      t.integer :certification_id
      t.integer :link_id

      t.timestamps
    end

    add_index :tests, :cm_test_id, unique: true
  end
end
