module DeviseHelper
  def devise_error_messages!
    return '' unless devise_error_messages?

    messages = resource.errors.full_messages.map { |msg| content_tag(:li, msg) }.join
    sentence = 'Uh Oh! Looks like there were some issues creating your account'

    html = <<-HTML
    <div id='error_explanation' class='ui error message'>
      <h2>#{sentence}</h2>
      <ul class='list'>#{messages}</ul>
    </div>
    HTML

    html.html_safe
  end

  def devise_error_messages?
    !resource.errors.empty?
  end

  def email_error?
    !resource.errors[:email].empty?
  end

  def password_error?
    !resource.errors[:password].empty?
  end

  def password_confirmation_error?
    !resource.errors[:password_confirmation].empty?
  end
end
