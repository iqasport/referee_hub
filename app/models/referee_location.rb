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

# Join table between Referees and NationalGoverningBody
class RefereeLocation < ApplicationRecord
  belongs_to :referee
  belongs_to :national_governing_body
end
