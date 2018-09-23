# == Schema Information
#
# Table name: certifications
#
#  id           :bigint(8)        not null, primary key
#  display_name :string           default(""), not null
#  level        :integer          not null
#  created_at   :datetime         not null
#  updated_at   :datetime         not null
#
# Indexes
#
#  index_certifications_on_level  (level) UNIQUE
#

class CertificationSerializer
  include FastJsonapi::ObjectSerializer

  attributes :level
end
