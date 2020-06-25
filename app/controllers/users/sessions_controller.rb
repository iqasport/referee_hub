# frozen_string_literal: true

class Users::SessionsController < Devise::SessionsController
  before_action :configure_sign_in_params, only: [:create]

  # GET /resource/sign_in
  def new
    super
  end

  # POST /resource/sign_in
  def create
    super
  end

  # DELETE /resource/sign_out
  def destroy
    super
  end

  protected

  # If you have extra params to permit, append them to the sanitizer.
  def configure_sign_in_params
    devise_parameter_sanitizer.permit(:sign_in, keys: [:policy_rule_privacy_terms])
  end

  def after_sign_in_path_for(_resource_or_scope)
    if current_user.enabled_features.include?('new_design')
      determine_redirect
    else
      return "/referees/#{current_user.id}"
    end
  end

  def after_sign_out_path_for(resource_or_scope)
    return "/sign_in"
  end

  private

  def determine_redirect
    roles = current_user.roles.map(&:access_type)
    return '/admin' if roles.include?('iqa_admin')
    return "/national_governing_bodies/#{current_user.owned_ngb&.first.id}" if roles.include?('ngb_admin')
    return "/referees/#{current_user.id}" if roles.include?('referee')

    return '/referees'
  end
end
