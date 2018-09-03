class CreateCertifications < ActiveRecord::Migration[5.2]
  def change
    create_table :certifications do |t|
      t.integer :level, null: false
      t.string :display_name, null: false, default: ''

      t.timestamps
    end

    add_index :certifications, :level, unique: true
  end
end
