class AddColumnsToTest < ActiveRecord::Migration[5.2]
  def change
    change_table :tests do |t|
      t.text :description, :null => false
      t.integer :time_limit, :null => false, :default => 18
      t.integer :minimum_pass_percentage, :null => false, :default => 80
      t.text :positive_feedback
      t.text :negative_feedback
      t.string :language
    end
  end
end
