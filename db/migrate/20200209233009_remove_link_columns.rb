class RemoveLinkColumns < ActiveRecord::Migration[5.2]
  def change
    remove_column :test_results, :cm_link_result_id
  end
end
