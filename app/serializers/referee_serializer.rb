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

class RefereeSerializer < BaseSerializer
  attributes :first_name,
             :last_name,
             :bio,
             :show_pronouns,
             :submitted_payment_at,
             :export_name,
             :avatar_url

  attribute :pronouns, if: proc { |user, params|
    current_user = params.present? && params[:current_user]
    user.show_pronouns || current_user && current_user.id == user.id
  }

  attribute :is_editable do |user, params|
    current_user = params.present? && params[:current_user]

    current_user&.id == user.id
  end

  attribute :has_pending_policies do |user, params|
    current_user = params.present? && params[:current_user]

    current_user&.id == user.id && user.pending_policies.present?
  end

  has_many :referee_locations, serializer: :referee_location
  has_many :national_governing_bodies, serializer: :national_governing_body, if: proc { |_referee, params| params[:include_associations] }
  has_many :referee_certifications, serializer: :referee_certification, if: proc { |_referee, params| params[:include_associations] }
  has_many :certifications, serializer: :certification, if: proc { |_referee, params| params[:include_associations] }
  has_many :test_results, if: proc { |_referee, params| params[:include_tests] }
  has_many :test_attempts, if: proc { |_referee, params| params[:include_tests] }
  has_many :teams, serializer: :team
  has_many :referee_teams, serializer: :referee_team
end
