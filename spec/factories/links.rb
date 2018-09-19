# == Schema Information
#
# Table name: links
#
#  id             :bigint(8)        not null, primary key
#  name           :string
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  cm_link_id     :integer
#  cm_link_url_id :string
#  test_id        :integer
#
# Indexes
#
#  index_links_on_cm_link_id  (cm_link_id) UNIQUE
#

require 'ffaker'

FactoryBot.define do
  factory :link do
    name { "#{FFaker::Job.title} Test" }
    test { create :test }
  end
end
