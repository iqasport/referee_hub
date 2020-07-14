# == Schema Information
#
# Table name: referee_locations
#
#  id                         :bigint           not null, primary key
#  association_type           :integer          default("primary")
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
  belongs_to :referee, class_name: 'User'
  belongs_to :national_governing_body

  enum association_type: {
    primary: 0,
    secondary: 1,
    other: 2
  }
end
