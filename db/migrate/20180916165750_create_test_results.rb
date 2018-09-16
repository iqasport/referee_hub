class CreateTestResults < ActiveRecord::Migration[5.2]
  def change
    create_table :test_results do |t|
      t.integer :cm_link_result_id
      t.integer :referee_id, null: false
      t.time :time_started
      t.time :time_finished
      t.string :duration
      t.integer :percentage
      t.integer :points_scored
      t.integer :points_available
      t.boolean :passed
      t.string :certificate_url
      t.integer :minimum_pass_percentage
      t.integer :link_id

      t.timestamps
    end

    add_index :test_results, :referee_id
  end
end
