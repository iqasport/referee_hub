class ApplicationController < ActionController::Base
  USER_UNAUTHORIZED = 'User must be an Admin to access this API'.freeze

  rescue_from Exception, with: :render_500
  rescue_from ActiveRecord::RecordInvalid, with: :render_unprocessable_entity_response
  rescue_from ActiveRecord::RecordNotFound, with: :render_not_found_response

  def render_unprocessable_entity_response(exception)
    render json: exception.record.errors, status: :unprocessable_entity
  end

  def render_not_found_response(exception)
    render json: { error: exception.message }, status: :not_found
  end

  def render_500(exception)
    render json: { error: exception.message }, status: :internal_server_error
  end

  private

  def after_sign_in_path_for(_resource_or_scope)
    "/referees/#{current_user.id}"
  end

  def verify_admin
    return true if current_user.iqa_admin?

    render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
  end

  def verify_ngb_admin
    return true if current_user.ngb_admin?

    render json: { error: USER_UNAUTHORIZED }, status: :unauthorized
  end
end
