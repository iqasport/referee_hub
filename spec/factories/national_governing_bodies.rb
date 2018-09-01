# == Schema Information
#
# Table name: national_governing_bodies
#
#  id         :bigint(8)        not null, primary key
#  name       :string           not null
#  website    :string
#  created_at :datetime         not null
#  updated_at :datetime         not null
#

require 'ffaker'

FactoryBot.define do
  factory :national_governing_body do
    name { FFaker::Address.country }
    website { FFaker::Internet.http_url }
  end
end
