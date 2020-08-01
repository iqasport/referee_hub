class AddJoinedAtToTeams < ActiveRecord::Migration[6.0]
  def change
    add_column :teams, :joined_at, :datetime, null: :false, default: -> { 'CURRENT_TIMESTAMP' }
  end
end
