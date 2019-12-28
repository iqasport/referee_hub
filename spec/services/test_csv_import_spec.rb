require 'csv'
require 'rails_helper'

describe Services::TestCsvImport do
  let(:test_to_import) { create :test }
  let(:headers) do
    'description,feedback,points_available,answer_1_description,answer_1_correct,answer_2_description,answer_2_correct'
  end
  let(:row1) { 'to be or not to be?,do better,1,be,true,not be,false' }
  let(:row2) { 'should you be the best?,also do better,1,yes,true,no,false' }
  let(:row3) { "are you already the best?,why can't you do better,1,yes,true,no,false" }
  let(:rows) { [headers, row1, row2, row3] }
  let(:file_path) { 'tmp/team_csv_import_test.csv' }
  let!(:csv) do
    CSV.open(file_path, 'w') do |csv|
      rows.each { |row| csv << row.split(',') }
    end
  end

  after(:each) { File.delete(file_path) }

  subject { described_class.new(file_path, test_to_import).perform }

  it 'creates 3 questions with answers' do
    expect { subject }.to change { test_to_import.questions.count }.by 3
  end

  it 'creates the answers for the provided questions' do
    subject

    expect(test_to_import.questions.first.answers.count).to eq 2
    expect(test_to_import.questions.first.answers.first.description).to eq 'be'
    expect(test_to_import.questions.first.answers.first.correct).to eq true
    expect(test_to_import.questions.first.answers.last.description).to eq 'not be'
    expect(test_to_import.questions.first.answers.last.correct).to eq false
  end

  context 'when test is not present' do
    let(:test_to_import) { nil }

    it 'raises a TestMissingError' do
      expect { subject }.to raise_error { Services::TestCsvImport::TestMissingError }
    end
  end
end
