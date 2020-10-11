source 'https://rubygems.org'
git_source(:github) { |repo| "https://github.com/#{repo}.git" }

ruby '2.7.1'

# Bundle edge Rails instead: gem 'rails', github: 'rails/rails'
gem 'rails', '~> 6.0.3', '>= 6.0.3.4'
# Use postgresql as the database for Active Record
gem 'pg', '>= 0.18', '< 2.0'
# Use Puma as the app server
gem 'puma', '~> 3.12'
# Use SCSS for stylesheets
gem 'sass-rails', '~> 5.1', '>= 5.1.0'
# Use Uglifier as compressor for JavaScript assets
gem 'uglifier', '>= 1.3.0'
gem 'json', '~> 2.3'

# Reduces boot times through caching; required in config/boot.rb
gem 'bootsnap', '>= 1.1.0', require: false

# authentication
gem 'devise', '>= 4.7.2'
gem 'devise_invitable', '~> 2.0.2'

# frontend
gem 'webpacker', '~> 5.1.1'

# api 
gem 'fast_jsonapi'
gem 'will_paginate', '~> 3.3.0'

# styles
gem 'semantic-ui-sass'
gem 'tailwindcss', '~> 1.0.3'
gem 'inline_svg'

# metrics and tracking
gem 'loofah', '>= 2.2.3'
gem 'rack', '2.1.4'
gem 'barnes', '0.0.8'
gem 'bugsnag', '~> 6.11'
gem 'scout_apm'

# job queuing
gem 'sidekiq'
gem 'sidekiq-status'
gem 'sidekiq-cron', '~> 1.1'

# misc
gem 'activerecord-import'
gem 'time_difference', git: 'https://github.com/iqasport/time_difference.git'
gem 'gdpr_rails', git: 'https://github.com/HipSpec/gdpr_rails.git', branch: 'rails-6-minor-travis-fix'
gem 'data_migrate', '>= 6.3.0'
gem 'stripe-rails', '>= 1.10.1'

# file storage and aws
gem 'aws-sdk-s3', '~> 1'
gem 'paperclip', '~> 6.0.0'
gem 'fog-aws'

# feature flippers
gem 'flipper'
gem 'flipper-ui'
gem 'flipper-active_record'
gem 'flipper-api'

group :development, :test do
  # Call 'byebug' anywhere in the code to stop execution and get a debugger console
  gem 'byebug', platforms: %i[mri mingw x64_mingw]
  gem 'annotate'
  gem 'factory_bot_rails', '~> 4.10', '>= 4.10.0'
  gem 'rspec-rails', '4.0.1'
  gem 'ffaker'
  gem 'colored'
end

group :development do
  # Access an interactive console on exception pages or by calling 'console' anywhere in the code.
  gem 'web-console', '>= 4.0.3'
  gem 'listen', '>= 3.0.5', '< 3.2'
  # Spring speeds up development by keeping your application running in the background. Read more: https://github.com/rails/spring
  gem 'spring'
  gem 'spring-watcher-listen', '~> 2.0.0'
  gem 'pre-commit', require: false
  gem 'rubocop', require: false
  gem 'solargraph', require: false
end

group :test do
  # Adds support for Capybara system testing and selenium driver
  gem 'capybara', '3.33.0'
  gem 'selenium-webdriver'
  # Easy installation and use of chromedriver to run system tests with Chrome
  gem 'chromedriver-helper'
  gem 'codecov', '~> 0.2.11', require: false
  gem 'timecop'
  gem 'stripe-ruby-mock'
end

# Windows does not include zoneinfo files, so bundle the tzinfo-data gem
gem 'tzinfo-data', platforms: %i[mingw mswin x64_mingw jruby]
