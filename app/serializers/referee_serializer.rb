class RefereeSerializer
  include FastJsonapi::ObjectSerializer

  attributes :first_name,
             :last_name,
             :bio,
             :email,
             :pronouns,
             :show_pronouns

  # has_many :national_governing_bodies
  # has_many :certifications
end
