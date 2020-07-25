class CreateCertificationPayments < ActiveRecord::Migration[6.0]
  def change
    create_table :certification_payments do |t|
      t.integer :user_id, null: false
      t.integer :certification_id, null: false
      t.string :stripe_session_id, null: false

      t.timestamps
    end
  end
end
