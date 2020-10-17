# == Schema Information
#
# Table name: tests
#
#  id                      :bigint           not null, primary key
#  active                  :boolean          default(FALSE), not null
#  description             :text             not null
#  language                :string
#  level                   :integer          default("snitch")
#  minimum_pass_percentage :integer          default(80), not null
#  name                    :string
#  negative_feedback       :text
#  positive_feedback       :text
#  recertification         :boolean          default(FALSE)
#  testable_question_count :integer          default(0), not null
#  time_limit              :integer          default(18), not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  certification_id        :integer
#  new_language_id         :integer
#

require 'ffaker'

FactoryBot.define do
  factory :test do
    level { 1 }
    name { "#{FFaker::Job.title} Test" }
    description { FFaker::Lorem.paragraph }
    testable_question_count { 5 }
    certification { create :certification }
  end
end
