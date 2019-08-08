web: bundle exec rails server -p $PORT
mailers: bundle exec sidekiq -v -c 5 -q mailers
release: bundle exec rails db:migrate
