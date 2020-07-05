# == Schema Information
#
# Table name: referee_certifications
#
#  id               :bigint           not null, primary key
#  needs_renewal_at :datetime
#  received_at      :datetime
#  renewed_at       :datetime
#  revoked_at       :datetime
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  certification_id :integer          not null
#  referee_id       :integer          not null
#
# Indexes
#
#  index_referee_certs_on_ref_id_and_cert_id  (referee_id,certification_id) UNIQUE WHERE (revoked_at IS NULL)
#

# A join table between referees and certifications. This model includes some business logic on when a referee is
# allowed to gain a certification
class RefereeCertification < ApplicationRecord
  FIELD_CERTIFICATION_ERROR = 'A Head Referee Certification is required before receiving a Field Certification'.freeze
  HEAD_CERTIFICATION_ERROR =
    'Both Snitch and Assistant Certifications are required before receiving a Head Referee Certification'.freeze
  CERTIFICATIONS_WITHOUT_PREREQS = %w[snitch assistant].freeze

  belongs_to :certification
  belongs_to :referee, class_name: 'User'

  validate :required_certifications, on: :create

  default_scope { where(revoked_at: nil) }
  scope :snitch, -> { joins(:certification).where(certification: { level: :snitch }) }
  scope :assistant, -> { joins(:certification).where(certification: { level: :assistant }) }
  scope :head, -> { joins(:certification).where(certification: { level: :head }) }
  scope :field, -> { joins(:certification).where(certification: { level: :field }) }

  def required_certifications
    return true if CERTIFICATIONS_WITHOUT_PREREQS.include?(certification.level)

    existing_certs = referee.certifications

    unless new_certification_valid?(existing_certs)
      errors.add(:certification, HEAD_CERTIFICATION_ERROR) if certification.level == 'head'
      errors.add(:certification, FIELD_CERTIFICATION_ERROR) if certification.level == 'field'
    end

    true
  end

  private

  def new_certification_valid?(existing_certs)
    if certification.level == 'head'
      existing_certs.snitch.count == 1 && existing_certs.assistant.count == 1
    elsif certification.level == 'field'
      existing_certs.head.count == 1
    end
  end
end
