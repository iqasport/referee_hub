class UserSerializer < BaseSerializer
  attributes :first_name, :last_name

  attribute :has_pending_policies do |user, params|
    current_user = params.present? && params[:current_user]

    current_user&.id == user.id && user.pending_policies.present?
  end

  has_many :roles
end
