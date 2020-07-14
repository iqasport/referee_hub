# == Schema Information
#
# Table name: social_accounts
#
#  id           :bigint           not null, primary key
#  account_type :integer          default("facebook"), not null
#  ownable_type :string
#  url          :string           not null
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#  ownable_id   :bigint
#
# Indexes
#
#  index_social_accounts_on_ownable_type_and_ownable_id  (ownable_type,ownable_id)
#

class SocialAccount < ApplicationRecord
  NAMED_ACCOUNT_TYPES = %w[facebook twitter youtube instagram].freeze

  enum account_type: {
    facebook: 0,
    twitter: 1,
    youtube: 2,
    instagram: 3,
    other: 4
  }

  belongs_to :ownable, polymorphic: true

  def self.match_account_type(url)
    matched_account_type = url.match(/\.\w+\./)[0].delete '.'
    return matched_account_type if NAMED_ACCOUNT_TYPES.include?(matched_account_type)

    'other'
  end
end
