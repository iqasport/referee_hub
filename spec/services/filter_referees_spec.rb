require 'rails_helper'
require 'json'

RSpec.describe Services::FilterReferees do
  let(:ngb) { create :national_governing_body }
  let(:cert) { create :certification }
  let(:referees) { create_list :user, 3 }
  let(:search_query) { 'test' }
  let(:certifications) { ['assistant'] }
  let(:ngbs) { [ngb.id] }
  let(:params) do
    {
      q: search_query,
      certifications: certifications,
      national_governing_bodies: ngbs
    }
  end

  before do
    referees.first.update(certifications: [cert])
    referees[1].update(first_name: 'test')
    referees.last.update(national_governing_bodies: [ngb])
  end

  subject { described_class.new(params).filter }

  context 'when only a search query exists' do
    let(:certifications) { nil }
    let(:ngbs) { nil }

    it 'should return the referee that matches the search query' do
      expect(subject.length).to eq 1
      expect(subject.first).to eq referees[1].id
    end
  end

  context 'when only a certification filter exists' do
    let(:search_query) { nil }
    let(:ngbs) { nil }

    it 'should return the referee that matches the filter query' do
      expect(subject.length).to eq 1
      expect(subject.first).to eq referees.first.id
    end
  end

  context 'when only a national governing body filter exists' do
    let(:search_query) { nil }
    let(:certifications) { nil }

    it 'should return the referee that matches the filter query' do
      expect(subject.length).to eq 1
      expect(subject.first).to eq referees.last.id
    end
  end

  context 'when certification and ngb fields exist' do
    let(:search_query) { nil }

    context 'and no referee meets both criteria' do
      it 'should not return a referee' do
        expect(subject.length).to eq 0
      end
    end

    context 'and a referee meets both criteria' do
      before { referees.last.update(certifications: [cert]) }

      it 'should return the matching referee' do
        expect(subject.length).to eq 1
        expect(subject.first).to eq referees.last.id
      end
    end
  end

  context 'when searching by name' do
    let!(:referee) { create :user }
    let(:search_query) { 'nap' }
    let(:params) { { q: search_query } }

    before { referee.update(first_name: first_name, last_name: last_name) }

    context 'when values are null' do
      context 'and first_name is null but last_name matches' do
        let(:first_name) { nil }
        let(:last_name) { 'Napperton' }

        it 'should return the referee' do
          expect(subject.length).to eq 1
          expect(subject.first).to eq referee.id
        end
      end

      context 'and last_name is null but first_name matches' do
        let(:first_name) { 'Nap' }
        let(:last_name) { nil }

        it 'should return the referee' do
          expect(subject.length).to eq 1
          expect(subject.first).to eq referee.id
        end
      end

      context 'and both first_name and last_name are null' do
        let(:first_name) { nil }
        let(:last_name) { nil }

        it 'should return an empty array' do
          expect(subject.length).to eq 0
        end
      end
    end

    context 'when values are present' do
      context 'and first name matches but last name doesnt' do
        let(:first_name) { 'Nap' }
        let(:last_name) { 'Blahburg' }

        it 'should return the referee' do
          expect(subject.length).to eq 1
          expect(subject.first).to eq referee.id
        end
      end

      context 'and last name matches but first name doesnt' do
        let(:first_name) { 'Blangdon' }
        let(:last_name) { 'Napperton' }

        it 'should return the referee' do
          expect(subject.length).to eq 1
          expect(subject.first).to eq referee.id
        end
      end

      context 'and neither name matches' do
        let(:first_name) { 'Mark' }
        let(:last_name) { 'Wahlberg' }

        it 'should return an empty array' do
          expect(subject.length).to eq 0
        end
      end
    end
  end

  context 'with different user types' do
    let(:search_query) { nil }
    let(:certifications) { nil }
    let(:ngbs) { nil }

    before { create_list(:user, 3, roles_attributes: [{ access_type: 'iqa_admin' }]) }

    it 'should return only referee user types' do
      expect(subject.length).to eq referees.length
    end
  end
end
