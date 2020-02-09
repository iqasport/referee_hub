class CreateExportedCsvs < ActiveRecord::Migration[5.2]
  def change
    create_table :exported_csvs do |t|
      t.string :type
      t.integer :user_id, null: false
      t.string :url
      t.datetime :processed_at
      t.datetime :sent_at
      t.json :export_options, null: false, default: {}

      t.timestamps
    end

    add_index :exported_csvs, :user_id
  end
end
