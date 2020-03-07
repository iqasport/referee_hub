class AddExportNameColumnToUser < ActiveRecord::Migration[5.2]
  def change
    add_column :users, :export_name, :boolean, default: true
  end
end
