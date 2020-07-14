# == Schema Information
#
# Table name: roles
#
#  id          :bigint           not null, primary key
#  access_type :integer          default("referee"), not null
#  created_at  :datetime         not null
#  updated_at  :datetime         not null
#  user_id     :bigint
#
# Indexes
#
#  index_roles_on_user_id                  (user_id)
#  index_roles_on_user_id_and_access_type  (user_id,access_type) UNIQUE
#
# Foreign Keys
#
#  fk_rails_...  (user_id => users.id)
#

class RoleSerializer < BaseSerializer
  attributes :access_type, :user_id
end
