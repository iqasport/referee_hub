class AddRecertificationToTests < ActiveRecord::Migration[6.0]
  def change
    add_column :tests, :recertification, :boolean, default: false
  end
end
