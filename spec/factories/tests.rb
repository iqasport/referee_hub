# == Schema Information
#
# Table name: tests
#
#  id                      :bigint(8)        not null, primary key
#  description             :text             not null
#  language                :string
#  level                   :integer          default("snitch")
#  minimum_pass_percentage :integer          default(80), not null
#  name                    :string
#  negative_feedback       :text
#  positive_feedback       :text
#  time_limit              :integer          default(18), not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  certification_id        :integer
#

require 'ffaker'

FactoryBot.define do
  factory :test do
    level 0
    name { "#{FFaker::Job.title} Test" }
    certification { create :certification, :snitch }
  end
end
