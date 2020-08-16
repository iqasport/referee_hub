require 'sidekiq'
require 'sidekiq-status'

redis_url = ENV['OPENREDIS_URL'] || ENV['REDIS_PROVIDER'] || ENV['REDISTOGO_URL'] || 'redis://127.0.0.1:6379/1'

Sidekiq.configure_client do |config|
  config.redis = { url: redis_url }

  Sidekiq::Status.configure_client_middleware config
end

Sidekiq.configure_server do |config|
  config.redis = { url: redis_url }

  Sidekiq::Status.configure_client_middleware config
  Sidekiq::Status.configure_server_middleware config

  schedule_file = "config/schedule.yml"

  if File.exist?(schedule_file) && Sidekiq.server?
    Sidekiq::Cron::Job.load_from_hash YAML.load_file(schedule_file)
  end
end
