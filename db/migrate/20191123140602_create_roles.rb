class CreateRoles < ActiveRecord::Migration[5.2]
  def change
    create_table :roles do |t|
      t.references :user, foreign_key: true
      t.integer :access_type, default: 0, null: false

      t.timestamps
    end

    add_index :roles, [:user_id, :access_type], unique: true
  end
end
