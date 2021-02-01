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
require 'rails_helper'
require 'stripe/rails/testing'

RSpec.describe CertificationPayment, type: :model do
  let!(:user) { create :user }
  let!(:cert) { create :certification }
  let(:user_email) { user.email }
  let(:created_payment) { CertificationPayment.last }

  before { StripeMock.start }

  subject do
    Stripe::Rails::Testing.send_event('checkout.session.completed', {
                                        metadata: {
                                          certification_id: cert.id
                                        },
                                        customer_email: user_email
                                      })
  end

  it 'should create a new certification payment' do
    subject

    expect(created_payment.user_id).to eq user.id
    expect(created_payment.certification_id).to eq cert.id
  end
end
