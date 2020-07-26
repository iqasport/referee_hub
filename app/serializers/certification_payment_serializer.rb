class CertificationPaymentSerializer < BaseSerializer
  attributes :user_id, :certification_id, :stripe_session_id
end
