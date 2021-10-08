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
    'Assistant, Snitch, and Scorekeeper Certifications are required before receiving a Head Referee Certification'.freeze
  SNITCH_CERTIFICATION_ERROR =
    'An Assistant Referee Certification is required before receiving a Snitch Certification'.freeze
  CERTIFICATIONS_WITHOUT_PREREQS = %w[assistant scorekeeper].freeze

  belongs_to :certification
  belongs_to :referee, class_name: 'User'

  validate :required_certifications, on: :create

  default_scope { where(revoked_at: nil) }
  scope :snitch, -> { joins(:certification).where(certification: { level: :snitch }) }
  scope :assistant, -> { joins(:certification).where(certification: { level: :assistant }) }
  scope :head, -> { joins(:certification).where(certification: { level: :head }) }
  scope :field, -> { joins(:certification).where(certification: { level: :field }) }
  scope :scorekeeper, -> { joins(:certification).where(certification: { level: :scorekeeper }) }

  def required_certifications
    return true if CERTIFICATIONS_WITHOUT_PREREQS.include?(certification.level)

    existing_certs = referee.certifications

    unless new_certification_valid?(existing_certs)
      errors.add(:certification, HEAD_CERTIFICATION_ERROR) if certification.level == 'head'
      errors.add(:certification, FIELD_CERTIFICATION_ERROR) if certification.level == 'field'
      errors.add(:certification, SNITCH_CERTIFICATION_ERROR) if certification.level == 'snitch'
    end

    true
  end

  private

  def new_certification_valid?(existing_certs)
    new_cert_version = certification.version
    return existing_certs.assistant.where(version: new_cert_version).count == 1 if certification.level == 'snitch'
    has_snitch_and_score = existing_certs.where(version: new_cert_version, level: %w[snitch scorekeeper]).count == 2
    # return has_snitch_and_score_or_recert if certification.level == 'head'
    # temp allow head certs to be issued if they've been allowed to take it
    return true if certification.level = 'head'
    return existing_certs.head.where(version: new_cert_version).count == 1 if certification.level == 'field'
  end
end
