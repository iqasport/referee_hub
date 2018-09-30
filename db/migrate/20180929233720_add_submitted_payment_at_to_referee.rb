class AddSubmittedPaymentAtToReferee < ActiveRecord::Migration[5.2]
  def change
    add_column :referees, :submitted_payment_at, :datetime
  end
end
