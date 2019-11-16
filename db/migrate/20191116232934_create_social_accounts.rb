class CreateSocialAccounts < ActiveRecord::Migration[5.2]
  def change
    create_table :social_accounts do |t|
      t.references :ownable, polymorphic: true
      t.string :url, null: false
      t.integer :account_type, null: false, default: 0

      t.timestamps
    end
  end
end
