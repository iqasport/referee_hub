class AddVersionToCertification < ActiveRecord::Migration[5.2]
  def change
    add_column :certifications, :version, :integer, default: 0
  end
end
