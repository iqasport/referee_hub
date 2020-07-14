# == Schema Information
#
# Table name: national_governing_bodies
#
#  id           :bigint           not null, primary key
#  acronym      :string
#  country      :string
#  image_url    :string
#  name         :string           not null
#  player_count :integer          default(0), not null
#  region       :integer
#  website      :string
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_national_governing_bodies_on_region  (region)
#

require 'rails_helper'

RSpec.describe NationalGoverningBody, type: :model do
  let(:ngb) { build :national_governing_body, name: nil }

  subject { ngb.save }

  it 'is an invalid record' do
    expect { subject }.to raise_error(ActiveRecord::NotNullViolation)
  end

  context 'with associated referees' do
    let(:ref_1) { create :user }
    let(:ref_2) { create :user }
    let(:ref_3) { create :user }
    let(:ngb) { build :national_governing_body }

    before { ngb.referees << [ref_1, ref_2] }

    it 'returns the correct referees' do
      subject
      expect(ngb.referees.pluck(:id)).to include(ref_1.id, ref_2.id)
    end
  end
end
