# == Schema Information
#
# Table name: social_accounts
#
#  id           :bigint(8)        not null, primary key
#  account_type :integer          default("facebook"), not null
#  ownable_type :string
#  url          :string           not null
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#  ownable_id   :bigint(8)
#
# Indexes
#
#  index_social_accounts_on_ownable_type_and_ownable_id  (ownable_type,ownable_id)
#

class SocialAccount < ApplicationRecord
  enum account_type: {
    facebook: 0,
    twitter: 1,
    youtube: 2,
    instagram: 3,
    other: 4
  }

  belongs_to :ownable, polymorphic: true
end
