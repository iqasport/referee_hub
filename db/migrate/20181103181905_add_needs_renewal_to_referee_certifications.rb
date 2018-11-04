class AddNeedsRenewalToRefereeCertifications < ActiveRecord::Migration[5.2]
  def change
    add_column :referee_certifications, :needs_renewal_at, :datetime
  end
end
