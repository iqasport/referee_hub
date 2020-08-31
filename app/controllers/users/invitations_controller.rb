# frozen_string_literal: true

class Users::InvitationsController < Devise::InvitationsController
  def update
    super do |resource|
      accept_params = devise_parameter_sanitizer.sanitize(:accept_invitation)
      if accept_params.fetch(:policy_rule_privacy_terms)
        resource.confirm_all_policies!
      else
        resource.reject_all_policies!
      end
      resource.confirm # set confirmed_at
      Flipper[:new_design].enable resource # allow the new user to have the new app
    end
  end
end
