class RenameRefereesToUsers < ActiveRecord::Migration[5.2]
  def change
    rename_table :referees, :users
  end
end
