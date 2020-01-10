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
#  failed_attempts              :integer          default(0), not null
#  first_name                   :string
#  getting_started_dismissed_at :datetime
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
#
# Indexes
#
#  index_users_on_confirmation_token    (confirmation_token) UNIQUE
#  index_users_on_email                 (email) UNIQUE
#  index_users_on_reset_password_token  (reset_password_token) UNIQUE
#  index_users_on_unlock_token          (unlock_token) UNIQUE
#

class User < ApplicationRecord
  include PolicyManager::Concerns::UserBehavior

  devise :database_authenticatable, :registerable,
         :recoverable, :rememberable, :trackable,
         :validatable, :confirmable, :lockable

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

  scope :certified, -> { joins(:certifications).group('referees.id') }
  scope :referee, -> { where(roles: { access_type: 'referee' }) }

  self.per_page = 25

  attr_accessor :disable_ensure_role, :policy_rule_privacy_terms

  after_save :ensure_role, on: :create

  def iqa_admin?
    roles.exists?(access_type: 'iqa_admin')
  end

  def policy_term_on(term = 'privacy_terms')
    super(term)
  end

  def policy_rule_privacy_terms; end

  def flipper_id
    "User;#{id}"
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
end
