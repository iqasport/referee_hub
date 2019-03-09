class AddTestIdToTestResult < ActiveRecord::Migration[5.2]
  def change
    add_column :test_results, :test_id
  end
end
