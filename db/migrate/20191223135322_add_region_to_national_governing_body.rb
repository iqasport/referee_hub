class AddRegionToNationalGoverningBody < ActiveRecord::Migration[5.2]
  def change
    add_column :national_governing_bodies, :region, :integer
    add_index :national_governing_bodies, :region
  end
end
