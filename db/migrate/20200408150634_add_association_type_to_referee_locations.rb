class AddAssociationTypeToRefereeLocations < ActiveRecord::Migration[5.2]
  def change
    add_column :referee_locations, :association_type, :integer, default: 0
  end
end
