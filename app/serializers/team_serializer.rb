# city                       :string           not null
#  country                    :string           not null
#  group_affiliation          :integer          default("university")
#  name                       :string           not null
#  state                      :string
#  status                     :integer          default("competitive")
#  created_at                 :datetime         not null
#  updated_at                 :datetime         not null
#  national_governing_body_id :bigint(8)
class TeamSerializer
  include FastJsonapi::ObjectSerializer

  attributes :city, :country, :group_affiliation, :name, :state, :status

  belongs_to :national_governing_body, serializer: :national_governing_body
end
