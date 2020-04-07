class RoleSerializer
  include FastJsonapi::ObjectSerializer

  attributes :access_type, :user_id
end
