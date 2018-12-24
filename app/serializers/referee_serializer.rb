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

class RefereeSerializer
  include FastJsonapi::ObjectSerializer

  attributes :first_name,
             :last_name,
             :bio,
             :email,
             :show_pronouns,
             :submitted_payment_at,
             :getting_started_dismissed_at

  attribute :pronouns, if: proc { |referee, params|
    current_user = params.present? && params[:current_user]
    referee.show_pronouns || current_user && current_user.id == referee.id
  }

  attribute :is_editable do |referee, params|
    current_user = params.present? && params[:current_user]
    return false unless current_user

    current_user.id == referee.id || current_user.admin?
  end

  has_many :national_governing_bodies, serializer: :national_governing_body
  has_many :referee_certifications, serializer: :referee_certification
  has_many :certifications, serializer: :certification
  has_many :test_results, if: proc { |_referee, params| params[:include_tests] }
  has_many :test_attempts, if: proc { |_referee, params| params[:include_tests] }
end
