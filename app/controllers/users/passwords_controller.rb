# frozen_string_literal: true

class Users::PasswordsController < Devise::PasswordsController
  # GET /resource/password/new
  def new
    super
  end

  # POST /resource/password
  def create
    super
  end

  # GET /resource/password/edit?reset_password_token=abcdef
  def edit
    super
  end

  # PUT /resource/password
  def update
    super do |resource|
      if resource_params.fetch(:policy_rule_privacy_terms)
        resource.confirm_all_policies!
      else
        resource.reject_all_policies!
      end
    end
  end

  # protected

  def configure_password_params
    devise_parameter_sanitizer.permit(:account_update, keys: [:policy_rule_privacy_terms])
  end

  def after_resetting_password_path_for(resource)
    super(resource)
  end

  # The path used after sending reset password instructions
  def after_sending_reset_password_instructions_path_for(resource_name)
    super(resource_name)
  end
end
