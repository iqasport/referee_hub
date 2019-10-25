require 'sidekiq'
require 'sidekiq-status'

redis_url = ENV['REDIS_PROVIDER'] || ENV['REDISTOGO_URL']

Sidekiq.configure_client do |config|
  config.redis = { url: redis_url }

  Sidekiq::Status.configure_client_middleware config
end

Sidekiq.configure_server do |config|
  config.redis = { url: redis_url }

  Sidekiq::Status.configure_client_middleware config
  Sidekiq::Status.configure_server_middleware config
end
