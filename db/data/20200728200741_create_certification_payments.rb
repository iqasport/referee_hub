class CreateCertificationPayments < ActiveRecord::Migration[6.0]
  def up
    users = User.all.where.not(submitted_payment_at: nil)
    certification = Certification.where(version: 'eighteen', level: 'head').first

    users.each do |user|
      CertificationPayment.create!(
        user: user,
        certification: certification,
        stripe_session_id: 'n/a',
        created_at: user.submitted_payment_at
      )
    end
  end

  def down
    raise ActiveRecord::IrreversibleMigration
  end
end
