class AddAdminToReferees < ActiveRecord::Migration[5.2]
  def change
    add_column :referees, :admin, :boolean, default: false
  end
end
