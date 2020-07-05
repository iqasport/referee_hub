class AddVersionUniquenessToCertification < ActiveRecord::Migration[6.0]
  def change
    remove_index :certifications, :level
    add_index :certifications, [:level, :version], unique: true
  end
end
