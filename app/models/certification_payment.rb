# == Schema Information
#
# Table name: certification_payments
#
#  id                :bigint           not null, primary key
#  created_at        :datetime         not null
#  updated_at        :datetime         not null
#  certification_id  :integer          not null
#  stripe_session_id :string           not null
#  user_id           :integer          not null
#
class CertificationPayment < ApplicationRecord
  include Stripe::Callbacks

  after_checkout_session_completed! do |checkout, event|
    user = User.find_by(email: checkout.customer_email)
    user = User.find_by(stripe_customer_id: checkout.customer) if user.blank?
    return if user.blank?

    CertificationPayment.create!(
      user_id: user.id,
      stripe_session_id: checkout.payment_intent,
      certification_id: checkout.metadata.certification_id
    )
  end

  belongs_to :user
  belongs_to :certification
end
