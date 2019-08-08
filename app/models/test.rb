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

# This model stores the test information sent by classmarker.
# It connects to our certification model to ensure the test result gives referees the right certification.
class Test < ApplicationRecord
  MAXIMUM_RETRIES = 6

  has_many :questions, dependent: :destroy
  has_many :referee_answers, dependent: :destroy
  has_many :test_attempts, dependent: :destroy
  has_many :test_results, dependent: :destroy

  enum level: {
    snitch: 0,
    assistant: 1,
    head: 2
  }

  scope :active, -> { where(active: true) }

  def fetch_random_questions
    questions.limit(testable_question_count).order(Arel.sql('RANDOM()'))
  end
end
