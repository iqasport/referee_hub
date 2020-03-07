# == Schema Information
#
# Table name: users
#
#  id                           :bigint(8)        not null, primary key
#  admin                        :boolean          default(FALSE)
#  bio                          :text
#  confirmation_sent_at         :datetime
#  confirmation_token           :string
#  confirmed_at                 :datetime
#  current_sign_in_at           :datetime
#  current_sign_in_ip           :inet
#  email                        :string           default(""), not null
#  encrypted_password           :string           default(""), not null
#  export_name                  :boolean          default(TRUE)
#  failed_attempts              :integer          default(0), not null
#  first_name                   :string
#  getting_started_dismissed_at :datetime
#  invitation_accepted_at       :datetime
#  invitation_created_at        :datetime
#  invitation_limit             :integer
#  invitation_sent_at           :datetime
#  invitation_token             :string
#  invitations_count            :integer          default(0)
#  invited_by_type              :string
#  last_name                    :string
#  last_sign_in_at              :datetime
#  last_sign_in_ip              :inet
#  locked_at                    :datetime
#  pronouns                     :string
#  remember_created_at          :datetime
#  reset_password_sent_at       :datetime
#  reset_password_token         :string
#  show_pronouns                :boolean          default(FALSE)
#  sign_in_count                :integer          default(0), not null
#  submitted_payment_at         :datetime
#  unlock_token                 :string
#  created_at                   :datetime         not null
#  updated_at                   :datetime         not null
#  invited_by_id                :bigint(8)
#
# Indexes
#
#  index_users_on_confirmation_token                 (confirmation_token) UNIQUE
#  index_users_on_email                              (email) UNIQUE
#  index_users_on_invitation_token                   (invitation_token) UNIQUE
#  index_users_on_invitations_count                  (invitations_count)
#  index_users_on_invited_by_id                      (invited_by_id)
#  index_users_on_invited_by_type_and_invited_by_id  (invited_by_type,invited_by_id)
#  index_users_on_reset_password_token               (reset_password_token) UNIQUE
#  index_users_on_unlock_token                       (unlock_token) UNIQUE
#

class User < ApplicationRecord
  include PolicyManager::Concerns::UserBehavior

  devise :invitable, :database_authenticatable, :registerable,
         :recoverable, :rememberable, :trackable,
         :validatable, :confirmable, :lockable

  # There is a uniqueness constraint on the national_governing_body_admin table on user_id so a single user can
  # not be an admin of more than on ngb. But an ngb can have multiple admins.
  has_many :national_governing_body_admins, dependent: :destroy
  has_many :owned_ngb, through: :national_governing_body_admins, source: :national_governing_body

  has_many :roles, dependent: :destroy
  accepts_nested_attributes_for :roles

  has_many :referee_locations, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy
  has_many :national_governing_bodies, through: :referee_locations

  has_many :referee_certifications, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy
  has_many :certifications, through: :referee_certifications

  has_many :test_results, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy
  has_many :test_attempts, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy
  has_many :referee_answers, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy

  has_many :referee_teams, foreign_key: :referee_id, inverse_of: :referee, dependent: :destroy
  has_many :teams, through: :referee_teams

  has_many :exported_csvs, dependent: :destroy

  scope :certified, -> { joins(:certifications).group('referees.id') }
  scope :referee, -> { where(roles: { access_type: 'referee' }) }

  self.per_page = 25

  attr_accessor :disable_ensure_role, :policy_rule_privacy_terms, :ngb_to_admin

  after_save :ensure_role, on: :create
  after_save :create_admin, if: proc { |user| user.ngb_to_admin.present? }

  def iqa_admin?
    roles.exists?(access_type: 'iqa_admin')
  end

  def ngb_admin?
    roles.exists?(access_type: 'ngb_admin')
  end

  def policy_term_on(term = 'privacy_terms')
    super(term)
  end

  def policy_rule_privacy_terms; end

  def flipper_id
    "User;#{id}"
  end

  def full_name
    "#{first_name.presence || ''} #{last_name.presence || ''}"
  end

  protected

  def confirmation_required?
    return true unless Rails.env.test?

    false
  end

  private

  def ensure_role
    return if disable_ensure_role.present?
    return if roles.present? || roles.referee.present?

    Role.create!(user_id: id, access_type: 'referee')
  end

  def create_admin
    return if ngb_admin?

    NationalGoverningBodyAdmin.create!(user: self, national_governing_body_id: ngb_to_admin)
  end
end
