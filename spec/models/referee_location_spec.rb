# == Schema Information
#
# Table name: referee_locations
#
#  id                         :bigint(8)        not null, primary key
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :integer          not null
#  referee_id                 :integer          not null
#
# Indexes
#
#  index_referee_locations_on_referee_id_and_ngb_id  (referee_id,national_governing_body_id) UNIQUE
#

require 'rails_helper'

RSpec.describe RefereeLocation, type: :model do
  let(:referee) { create :referee }
  let(:ngb) { create :national_governing_body }
  let(:ref_location) { build :referee_location, referee: referee, national_governing_body: ngb }

  subject { ref_location.save }

  it 'saves the association correctly' do
    subject
    expect(ref_location.referee).to eq referee
    expect(ref_location.national_governing_body).to eq ngb
  end
end
