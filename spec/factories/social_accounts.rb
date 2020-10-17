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

FactoryBot.define do
  factory :social_account do
    ownable_type { 'Team' }
    ownable_id { 1 }
    url { 'www.facebook.com' }
    account_type { 1 }
  end
end
