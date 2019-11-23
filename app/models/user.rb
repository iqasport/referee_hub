# == Schema Information
#
# Table name: users
#
#  id                           :bigint(8)        not null, primary key
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
  devise :database_authenticatable, :registerable,
         :recoverable, :rememberable, :trackable, :validatable, :confirmable, :lockable

  has_many :roles, dependent: :destroy

  protected

  def confirmation_required?
    return true unless Rails.env.test?

    false
  end
end
