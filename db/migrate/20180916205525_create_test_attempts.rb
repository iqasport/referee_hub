class CreateTestAttempts < ActiveRecord::Migration[5.2]
  def change
    create_table :test_attempts do |t|
      t.integer :test_id
      t.integer :referee_id
      t.integer :test_level
      t.datetime :next_attempt_at

      t.timestamps
    end
  end
end
