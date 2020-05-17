class AddTotalTeamsCountToNationalGoverningBodyStats < ActiveRecord::Migration[5.2]
  def change
    add_column :national_governing_body_stats, :total_teams_count, :integer, default: 0
    add_column :national_governing_body_stats, :uncertified_count, :integer, default: 0
    add_column :national_governing_body_stats, :start, :datetime
    add_column :national_governing_body_stats, :end, :datetime
  end
end
