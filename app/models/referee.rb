# == Schema Information
#
# Table name: referees
#
#  id                           :bigint(8)        not null, primary key
#  bio                          :text
#  current_sign_in_at           :datetime
#  current_sign_in_ip           :inet
#  email                        :string           default(""), not null
#  encrypted_password           :string           default(""), not null
#  first_name                   :string
#  getting_started_dismissed_at :datetime
#  last_name                    :string
#  last_sign_in_at              :datetime
#  last_sign_in_ip              :inet
#  pronouns                     :string
#  remember_created_at          :datetime
#  reset_password_sent_at       :datetime
#  reset_password_token         :string
#  show_pronouns                :boolean          default(FALSE)
#  sign_in_count                :integer          default(0), not null
#  submitted_payment_at         :datetime
#  created_at                   :datetime         not null
#  updated_at                   :datetime         not null
#
# Indexes
#
#  index_referees_on_email                 (email) UNIQUE
#  index_referees_on_reset_password_token  (reset_password_token) UNIQUE
#

# Referee is the core model of this application many of the attributes are used by Devise to authenticate the user.
class Referee < ApplicationRecord
  devise :database_authenticatable, :registerable,
         :recoverable, :rememberable, :trackable, :validatable

  has_many :referee_locations, dependent: :destroy
  has_many :national_governing_bodies, through: :referee_locations

  has_many :referee_certifications, dependent: :destroy
  has_many :certifications, through: :referee_certifications

  has_many :test_results, dependent: :destroy
  has_many :test_attempts, dependent: :destroy

  scope :certified, -> { joins(:certifications).group('referees.id') }

  self.per_page = 25
end
