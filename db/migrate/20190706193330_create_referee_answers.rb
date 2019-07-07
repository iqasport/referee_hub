class CreateRefereeAnswers < ActiveRecord::Migration[5.2]
  def change
    create_table :referee_answers do |t|
      t.references :referee, null: false
      t.references :test, null: false
      t.references :question, null: false
      t.references :answer, null: false
      t.references :test_attempt, null: false

      t.timestamps
    end
  end
end
