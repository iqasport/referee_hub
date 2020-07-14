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

class RefereeLocationSerializer < BaseSerializer
  attributes :referee_id, :association_type, :national_governing_body_id
end
