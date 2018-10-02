class AddTestLevelToTestResult < ActiveRecord::Migration[5.2]
  def change
    add_column :test_results, :test_level, :integer, default: 0
    remove_column :test_results, :link_id
  end
end
