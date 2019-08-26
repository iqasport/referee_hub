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
  require 'csv'
  require 'activerecord-import/base'
  require 'activerecord-import/active_record/adapters/postgresql_adapter'

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

  def self.csv_import(file) # rubocop:disable Metrics/MethodLength
    tests = []

    CSV.foreach(file.path, headers: true) do |row|
      row_data = row.to_h.with_indifferent_access
      all_keys = row_data.keys
      new_test = Test.new(
        description: row_data.dig(:description),
        language: row_data.dig(:language),
        level: row_data.dig(:level),
        minimum_pass_percentage: row_data.dig(:minimum_pass_percentage),
        name: row_data.dig(:name),
        negative_feedback: row_data.dig(:negative_feedback),
        positive_feedback: row_data.dig(:positive_feedback),
        testable_question_count: row_data.dig(:question_count),
        time_limit: row_data.dig(:time_limit)
      )
      next unless new_test.valid?

      question_keys = all_keys.select { |key| key =~ /question_\d$/ }
      questions = []
      question_keys.each do |question_key|
        new_question = Question.new(
          description: row_data["#{question_key}_description"],
          feedback: row_data["#{question_key}_feedback"],
          points_available: row_data["#{question_key}_points_available"]
        )
        next unless new_question.valid?

        question_number = question_key[-1].to_i
        answer_keys = all_keys.select {|key| key =~ /answer_#{question_number}\w$/}
        answers = []
        answer_keys.each do |answer_key|
          new_answer = Answer.new(
            description: row_data["#{answer_key}_description"],
            correct: row_data["#{answer_key}_correct"]
          )
          next unless new_answer.valid?

          answers << new_answer
        end

        new_question.answers = answers
        questions << new_question
      end

      new_test.questions = questions
      tests << new_test
    end

    Test.import tests, recursive: true
  end

  def fetch_random_questions
    questions.limit(testable_question_count).order(Arel.sql('RANDOM()'))
  end
end
