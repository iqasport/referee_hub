class AddScorekeeperRefereesCountToNationalGoverningBodyStat < ActiveRecord::Migration[6.0]
  def change
    add_column :national_governing_body_stats, :scorekeeper_referees_count, :integer, default: 0
  end
end
