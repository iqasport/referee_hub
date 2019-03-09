class CreateAnswers < ActiveRecord::Migration[5.2]
  def change
    create_table :answers do |t|
      t.integer :question_id, :null => false
      t.text :description, :null => false
      t.boolean :correct, :null => false, :default => false
    end
  end
end
