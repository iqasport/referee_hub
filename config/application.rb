require_relative 'boot'

require 'rails/all'
require 'policy_manager'

# Require the gems listed in Gemfile, including any gems
# you've limited to :test, :development, or :production.
Bundler.require(*Rails.groups)

module RefereeHub
  class Application < Rails::Application
    # Initialize configuration defaults for originally generated Rails version.
    config.load_defaults 5.2

    # Settings in config/environments/* take precedence over those specified here.
    # Application configuration can go into files in config/initializers
    # -- all .rb files in that directory are automatically loaded after loading
    # the framework and any gems in your application.
    config.enable_dependency_loading = true
    config.autoload_paths += %W[#{config.root}/app #{config.root}/app/services #{config.root}/lib]
    config.active_job.queue_adapter = :sidekiq
    config.stripe.publishable_key = ENV['STRIPE_PUBLISHABLE_KEY']
    config.stripe.secret_key = ENV['STRIPE_SECRET_KEY']
    config.stripe.signing_secrets = ['whsec_VODTQ2agBWcqUSskoPQzLT4iX8eLXMKJ']
  end
end
