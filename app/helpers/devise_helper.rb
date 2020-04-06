module DeviseHelper
  def devise_error_messages!
    return '' unless devise_error_messages?

    html = <<-HTML
    <div id='error_explanation' class='error-message'>
      <h2>#{fetch_sentence}</h2>
      <ul class='list px-8'>#{fetch_messages}</ul>
    </div>
    HTML

    html.html_safe
  end

  def devise_error_messages?
    !resource.errors.empty? || flash_errors?
  end

  def flash_errors?
    flash.present?
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

  private

  def fetch_messages
    if flash_errors?
      content_tag(:li, flash[:alert])
    else
      resource.errors.full_messages.map { |msg| content_tag(:li, msg) }.join
    end
  end

  def fetch_sentence
    return '' if flash_errors?

    'Uh Oh! Looks like there were some issues with your account'
  end
end
