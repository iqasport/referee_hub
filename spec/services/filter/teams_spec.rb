require 'rails_helper'

RSpec.describe Services::Filter::Teams do
  let!(:ngb) { create :national_governing_body }
  let!(:teams) { create_list :team, 4, national_governing_body: ngb }
  let!(:other_teams) { create_list :team, 5 }
  let(:search_query) { 'test qc' }
  let(:ngbs) { [ngb.id] }
  let(:status) { ['competitive'] }
  let(:group_affiliation) { ['community'] }
  let(:params) do
    {
      q: search_query,
      national_governing_bodies: ngbs,
      status: status,
      group_affiliation: group_affiliation
    }
  end

  before do
    teams.first.update!(name: search_query) if search_query.present?
    teams[1].update!(status: 'developing')
    teams[2].update!(group_affiliation: 'community')
  end

  subject { described_class.new(params).filter }

  context 'when only a search query exists' do
    let(:ngbs) { nil }
    let(:status) { nil }
    let(:group_affiliation) { nil }

    it 'should return the matching team' do
      expect(subject.length).to eq 1
      expect(subject.first).to eq teams.first.id
    end

    context 'when the query is capitalized' do
      let(:search_query) { 'First QC' }

      before { teams.first.update(name: 'first qc') }

      it 'should return the matching team' do
        expect(subject.length).to eq 1
        expect(subject.first).to eq teams.first.id
      end
    end

    context 'when the query is lowercased' do
      let(:search_query) { 'first qc' }

      before { teams.first.update(name: 'First QC') }

      it 'should return the matching team' do
        expect(subject.length).to eq 1
        expect(subject.first).to eq teams.first.id
      end
    end
  end

  context 'when only the ngb filter exists' do
    let(:status) { nil }
    let(:group_affiliation) { nil }
    let(:search_query) { nil }

    it 'should return all of the teams that belong to the ngb' do
      expect(subject.length).to eq teams.length
    end

    context 'when there is more than one ngb' do
      let(:ngbs) { [ngb.id, other_teams.first.national_governing_body.id] }

      it 'should return all of the teams that belong to the ngbs' do
        expect(subject.length).to eq 5
      end
    end
  end

  context 'when only the status filter exists' do
    let(:group_affiliation) { nil }
    let(:search_query) { nil }
    let(:ngbs) { nil }

    it 'should return all teams with that status' do
      # in the before lifecycle hook we only change 1 team status. The rest default to the current status filter
      expect(subject.length).to eq 8
    end

    context 'when multiple statuses are present' do
      let(:status) { %w[competitive developing] }

      it 'should return all teams with that status' do
        expect(subject.length).to eq teams.length + other_teams.length
      end
    end
  end

  context 'when only the group_affiliation filter exists' do
    let(:status) { nil }
    let(:search_query) { nil }
    let(:ngbs) { nil }

    it 'should return all teams with that group_affiliation' do
      expect(subject.length).to eq 1
    end

    context 'when multiple group_affiliations are present' do
      let(:group_affiliation) { %w[community university] }

      it 'should return all teams with that group_affiliation' do
        expect(subject.length).to eq teams.length + other_teams.length
      end
    end
  end

  context 'when multiple filters exist' do
    let(:group_affiliation) { nil }
    let(:search_query) { nil }
    let(:status) { ['developing'] }

    it 'should return all of the teams that match all filters' do
      expect(subject.length).to eq 1
      expect(subject.first).to eq teams[1].id
    end
  end

  context 'when no filters exist' do
    let(:status) { nil }
    let(:search_query) { nil }
    let(:ngbs) { nil }
    let(:group_affiliation) { nil }

    it 'should return the base relation' do
      expect(subject.count).to eq teams.length + other_teams.length
    end
  end
end
