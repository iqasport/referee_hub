require 'rails_helper'

RSpec.describe ExportedCsv::TestExport, type: :model do
  let(:user) { create :user }
  let(:csv) { user.exported_csvs.last }
  let!(:test) { create :test }
  let!(:questions) { create_list :question, 5, test_id: test.id }
  let(:export_options) { { test_id: test.id }.to_json }
  let(:question) { questions.first }
  let(:correct_index) { 0 }

  before do
    questions.each do |q|
      create_list(:answer, 4, question_id: q.id)
      q.answers.each_with_index {|a, idx| a.update(correct: true) if idx == correct_index }
    end
  end

  subject { described_class.new(user: user, export_options: export_options) }

  it 'builds a question row' do
    expect(subject.build_question_row(question)).to eq(
      [
        question.description,
        question.points_available,
        "Answer #{correct_index + 1}"
      ].push(question.answers.map {|a| a.description }).flatten
    )
  end

  it 'generates csv data' do
    expect(subject.generate_csv_data).to_not be_nil
  end

  context 'without any correct answers' do
    let(:correct_index) { 100 }

    it 'builds the question row' do
      expect(subject.build_question_row(question)).to eq(
        [
          question.description,
          question.points_available,
          ''
        ].push(question.answers.map {|a| a.description }).flatten
      )
    end
  end

  context 'without a test_id' do
    let(:export_options) { {}.to_json }

    it 'raises an error' do
      expect { subject.generate_csv_data }.to raise_error(StandardError)
    end
  end
end
