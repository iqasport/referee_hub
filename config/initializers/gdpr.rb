PolicyManager::Config.setup do |c|
  c.logout_url = '/sign_out'
  c.from_email = 'noreply@iqareferees.org'

  c.user_resource = User
  c.is_admin_method = ->(user){ user.iqa_admin? }

  c.add_rule({name: "privacy_terms", validates_on: [:create, :update], blocking: true })
end

PolicyManager::UserTermsController.send(:include, Devise::Controllers::Helpers)
PolicyManager::ApplicationController.send(:include, Devise::Controllers::Helpers)
