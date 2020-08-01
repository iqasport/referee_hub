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
class CertificationPaymentSerializer < BaseSerializer
  attributes :user_id, :certification_id, :stripe_session_id
end
