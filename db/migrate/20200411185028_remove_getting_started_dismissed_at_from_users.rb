class RemoveGettingStartedDismissedAtFromUsers < ActiveRecord::Migration[5.2]
  def change
    remove_column :users, :getting_started_dismissed_at
  end
end
