# == Schema Information
#
# Table name: tests
#
#  id                      :bigint(8)        not null, primary key
#  active                  :boolean          default(FALSE), not null
#  description             :text             not null
#  language                :string
#  level                   :integer          default("snitch")
#  minimum_pass_percentage :integer          default(80), not null
#  name                    :string
#  negative_feedback       :text
#  positive_feedback       :text
#  testable_question_count :integer          default(0), not null
#  time_limit              :integer          default(18), not null
#  created_at              :datetime         not null
#  updated_at              :datetime         not null
#  certification_id        :integer
#

class TestSerializer
  include FastJsonapi::ObjectSerializer

  attributes :description,
             :language,
             :level,
             :minimum_pass_percentage,
             :name,
             :negative_feedback,
             :positive_feedback,
             :time_limit,
             :active,
             :testable_question_count
end
