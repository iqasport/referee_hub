class RefereeSerializer
  include FastJsonapi::ObjectSerializer

  attributes :first_name,
             :last_name,
             :bio,
             :email

  attribute :pronouns, if: proc { |referee|
    referee.show_pronouns
  }

  has_many :national_governing_bodies
  has_many :certifications
end
