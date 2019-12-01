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

FactoryBot.define do
  factory :referee_location do
    referee { create :user }
    national_governing_body { create :national_governing_body }
  end
end
