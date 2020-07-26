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
FactoryBot.define do
  factory :certification_payment do
    user { create :user }
    certification { create :certification }
    stripe_session_id { FFaker::Identification.drivers_license }
  end
end
