class AddLanguageIdToTest < ActiveRecord::Migration[6.0]
  def change
    add_column :tests, :new_language_id, :integer
  end
end
