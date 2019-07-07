class AddTestableQuestionCountToTests < ActiveRecord::Migration[5.2]
  def change
    add_column :tests, :testable_question_count, :integer, default: 0, null: false
  end
end
