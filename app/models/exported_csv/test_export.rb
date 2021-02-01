# == Schema Information
#
# Table name: exported_csvs
#
#  id             :bigint           not null, primary key
#  export_options :json             not null
#  processed_at   :datetime
#  sent_at        :datetime
#  type           :string
#  url            :string
#  created_at     :datetime         not null
#  updated_at     :datetime         not null
#  user_id        :integer          not null
#
# Indexes
#
#  index_exported_csvs_on_user_id  (user_id)
#
require 'csv'

class ExportedCsv::TestExport < ExportedCsv
  COLUMN_HEADERS = [
    'Question Description',
    'Points Available',
    'Correct Answer',
    'Answer 1',
    'Answer 2',
    'Answer 3',
    'Answer 4'
  ].freeze

  def generate_csv_data
    test_id = filter_options.to_h.with_indifferent_access.delete(:test_id)
    raise StandardError, 'Test ID is required' unless test_id.present?

    export_test = Test.find(test_id)

    CSV.generate(col_sep: ',', quote_char: '"') do |csv|
      csv << COLUMN_HEADERS

      export_test.questions.includes(:answers).each do |question|
        csv << build_question_row(question)
      end
    end
  end

  def build_question_row(question)
    answers = question.answers.to_a
    correct_answer_index = answers.index { |answer| answer.correct }
    answer_descs = answers.map { |answer| answer.description }
    correct_answer_text = correct_answer_index.present? ? "Answer #{correct_answer_index + 1}" : ''

    [
      question.description,
      question.points_available,
      correct_answer_text
    ].push(answer_descs).flatten
  end

  private

  def filter_options
    JSON.parse(export_options)
  end
end
