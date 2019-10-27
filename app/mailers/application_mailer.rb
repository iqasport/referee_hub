class ApplicationMailer < ActionMailer::Base
  add_template_helper(EmailHelper)
  default from: 'noreply@iqareferees.org'
  layout 'mailer'
end
