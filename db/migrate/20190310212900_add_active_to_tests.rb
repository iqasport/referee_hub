class AddActiveToTests < ActiveRecord::Migration[5.2]
  def change
    add_column :tests, :active, :boolean, :null => false, :default => false
  end
end
