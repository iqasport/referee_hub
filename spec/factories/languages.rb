# == Schema Information
#
# Table name: languages
#
#  id           :bigint           not null, primary key
#  long_name    :string           default("english"), not null
#  long_region  :string
#  short_name   :string           default("en"), not null
#  short_region :string
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
FactoryBot.define do
  factory :language do
    long_name { 'english' }
    short_name { 'en' }
    long_region { '' }
    short_region { '' }
  end
end
