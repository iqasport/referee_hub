require 'csv'

module Services
  class TestCsvImport
    attr_reader :file_path, :test_to_import, :mapped_headers
    attr_accessor :questions

    TestMissingError = Class.new(StandardError)
    def initialize(file_path, test_to_import, mapped_headers)
      @file_path = file_path
      @test_to_import = test_to_import
      @mapped_headers = ActiveSupport::JSON.decode(mapped_headers).with_indifferent_access
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

      import_results = Question.import questions, recursive: true, returning: :description
      import_results.ids
    end

    private

    def build_new_question(row_data)
      Question.new(
        description: row_data.dig(mapped_headers['description']),
        feedback: row_data.dig(mapped_headers['feedback']),
        points_available: row_data.dig(mapped_headers['points_available']),
        test_id: test_to_import.id
      )
    end

    def build_new_answer(row_data, answer_key)
      correct = row_data.dig(mapped_headers['correct_answer']) == answer_key

      Answer.new(
        description: row_data.dig(answer_key),
        correct: correct
      )
    end

    def build_test_question(row_data, all_keys)
      new_question = build_new_question(row_data)

      mapped_keys = mapped_headers.keys.select { |key| key =~ /answer_\d+/ }
      answer_keys = mapped_keys.select { |key| mapped_headers[key] }

      answers = []
      answer_keys.each { |answer_key| answers << build_new_answer(row_data, answer_key) }

      new_question.answers = answers
      new_question
    end
  end
end
