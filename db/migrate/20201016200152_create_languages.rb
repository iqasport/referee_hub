class CreateLanguages < ActiveRecord::Migration[6.0]
  def change
    create_table :languages do |t|
      t.string :long_name, null: false, default: 'english'
      t.string :short_name, null: false, default: 'en'
      t.string :long_region
      t.string :short_region

      t.timestamps
    end
  end
end
