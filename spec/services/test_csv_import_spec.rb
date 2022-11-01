require 'csv'
require 'rails_helper'

describe Services::TestCsvImport do
  let(:test_to_import) { create :test }
  let(:headers) do
    'description,feedback,points_available,correct_answer,answer_1,answer_2,answer_3,answer_4'
  end
  let(:row1) { 'to be or not to be?,do better,1,answer_1,be,maybe,not be,never' }
  let(:row2) { 'should you be the best?,also do better,1,answer_2,yes,sometimes,no,never' }
  let(:row3) { "are you already the best?,why can't you do better,1,answer_3,possibly,maybe,kind of,never" }
  let(:rows) { [headers, row1, row2, row3] }
  let(:file_path) { 'tmp/team_csv_import_test.csv' }
  let!(:csv) do
    CSV.open(file_path, 'w') do |csv|
      rows.each { |row| csv << row.split(',') }
    end
  end
  let(:mapped_headers) do
    {
      'description': 'description',
      'feedback': 'feedback',
      'points_available': 'points_available',
      'correct_answer': 'correct_answer',
      'answer_1': 'answer_1',
      'answer_2': 'answer_2',
      'answer_3': 'answer_3',
      'answer_4': 'answer_4'
    }
  end

  after(:each) { File.delete(file_path) }

  subject { described_class.new(file_path, test_to_import, mapped_headers.to_json).perform }

  it 'creates 3 questions with answers' do
    expect { subject }.to change { test_to_import.questions.count }.by 3
  end

  it 'creates the answers for the provided questions' do
    subject

    expect(test_to_import.questions.first.answers.count).to eq 4
    expect(test_to_import.questions.first.answers.pluck(:description)).to eq ['be', 'maybe', 'not be', 'never']
    expect(test_to_import.questions.first.answers.first.correct).to eq true
    expect(test_to_import.questions.first.answers.last.correct).to eq false
  end

  context 'when test is not present' do
    let(:test_to_import) { nil }

    it 'raises a TestMissingError' do
      expect { subject }.to raise_error { Services::TestCsvImport::TestMissingError }
    end
  end

  context 'with custom headers' do
    let(:headers) do
      'q description,feedback,points available,correct answer,answer 1,answer 2,answer 3,answer 4'
    end
    let(:row1) { 'to be or not to be?,do better,1,answer 1,be,maybe,not be,never' }
    let(:row2) { 'should you be the best?,also do better,1,answer 2,yes,sometimes,no,never' }
    let(:row3) { "are you already the best?,why can't you do better,1,answer 3,possibly,maybe,kind of,never" }
    let(:rows) { [headers, row1, row2, row3] }
    let!(:csv) do
      CSV.open(file_path, 'w') do |csv|
        rows.each { |row| csv << row.split(',') }
      end
    end
    let(:mapped_headers) do
      {
        'description': 'q description',
        'feedback': 'feedback',
        'points_available': 'points available',
        'correct_answer': 'correct answer',
        'answer_1': 'answer 1',
        'answer_2': 'answer 2',
        'answer_3': 'answer 3',
        'answer_4': 'answer 4'
      }
    end

    subject { described_class.new(file_path, test_to_import, mapped_headers.to_json).perform }

    it 'creates 3 questions with answers' do
      expect { subject }.to change { test_to_import.questions.count }.by 3
    end
  
    it 'creates the answers for the provided questions' do
      subject
  
      expect(test_to_import.questions.first.answers.count).to eq 4
      expect(test_to_import.questions.first.answers.pluck(:description)).to eq ['be', 'maybe', 'not be', 'never']
      expect(test_to_import.questions.first.answers.first.correct).to eq true
      expect(test_to_import.questions.first.answers.last.correct).to eq false
    end
  end
end
