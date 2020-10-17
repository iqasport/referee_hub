class AddLanguageIdToUser < ActiveRecord::Migration[6.0]
  def change
    add_column :users, :language_id, :integer
  end
end
