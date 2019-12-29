require 'csv'

module Services
  class TestCsvImport
    attr_reader :file_path, :test_to_import
    attr_accessor :questions

    TestMissingError = Class.new(StandardError)
    def initialize(file_path, test_to_import)
      @file_path = file_path
      @test_to_import = test_to_import
      @questions = []
    end

    def perform
      raise TestMissingError, 'Test must be present to import questions and answers' if test_to_import.blank?

      CSV.foreach(file_path, headers: true) do |row|
        row_data = row.to_h.with_indifferent_access
        all_keys = row_data.keys

        question = build_test_question(row_data, all_keys)
        questions << question
      end

      Question.import questions, recursive: true
    end

    private

    def build_new_question(row_data)
      Question.new(
        description: row_data.dig('description'),
        feedback: row_data.dig('feedback'),
        points_available: row_data.dig('points_available'),
        test_id: test_to_import.id
      )
    end

    def build_new_answer(row_data, answer_key)
      Answer.new(
        description: row_data.dig("#{answer_key}_description"),
        correct: row_data.dig("#{answer_key}_correct")
      )
    end

    def build_test_question(row_data, all_keys)
      new_question = build_new_question(row_data)

      answer_keys = all_keys.select { |key| key =~ /answer_\d/ }
      answer_numbers = answer_keys.map { |key| key.gsub(/_\D+/, '') }.uniq!
      answers = []
      answer_numbers.each { |answer_key| answers << build_new_answer(row_data, answer_key) }

      new_question.answers = answers
      new_question
    end
  end
end
