class AddGettingStartedDismissedAtToReferee < ActiveRecord::Migration[5.2]
  def change
    add_column :referees, :getting_started_dismissed_at, :datetime
  end
end
