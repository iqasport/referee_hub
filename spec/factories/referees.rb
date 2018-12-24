# == Schema Information
#
# Table name: referees
#
#  id                           :bigint(8)        not null, primary key
#  admin                        :boolean          default(FALSE)
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

require 'ffaker'

FactoryBot.define do
  factory :referee do
    first_name { FFaker::Name.first_name }
    last_name { FFaker::Name.last_name }
    email { "#{first_name}.#{last_name}@example.com" }
    password { 'password' }
  end
end
