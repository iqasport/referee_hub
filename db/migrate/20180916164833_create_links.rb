class CreateLinks < ActiveRecord::Migration[5.2]
  def change
    create_table :links do |t|
      t.integer :cm_link_id
      t.string :name
      t.string :cm_link_url_id
      t.integer :test_id

      t.timestamps
    end

    add_index :links, :cm_link_id, unique: true
  end
end
