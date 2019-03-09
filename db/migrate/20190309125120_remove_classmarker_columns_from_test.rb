class RemoveClassmarkerColumnsFromTest < ActiveRecord::Migration[5.2]
  def change
    remove_column :tests, :cm_test_id, :integer
    remove_column :tests, :link_id, :integer
  end
end
