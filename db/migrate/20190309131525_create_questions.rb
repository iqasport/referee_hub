class CreateQuestions < ActiveRecord::Migration[5.2]
  def change
    create_table :questions do |t|
      t.integer :test_id, :null => false
      t.text :description, :null => false
      t.integer :points_available, :null => false, :default => 1
      t.text :feedback
    end
  end
end
