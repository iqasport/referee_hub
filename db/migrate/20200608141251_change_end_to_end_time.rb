class ChangeEndToEndTime < ActiveRecord::Migration[5.2]
  def up
    rename_column :national_governing_body_stats, :end, :end_time
  end

  def down
    rename_column :national_governing_body_stats, :end_time, :end
  end
end
