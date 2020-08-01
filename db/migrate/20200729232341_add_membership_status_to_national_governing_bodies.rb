class AddMembershipStatusToNationalGoverningBodies < ActiveRecord::Migration[6.0]
  def change
    add_column :national_governing_bodies, :membership_status, :integer, null: false, default: 0
  end
end
