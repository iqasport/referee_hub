# README
[![CircleCI](https://circleci.com/gh/iqasport/referee_hub/tree/master.svg?style=svg)](https://circleci.com/gh/iqasport/referee_hub/tree/master)
[![codecov](https://codecov.io/gh/iqasport/referee_hub/branch/master/graph/badge.svg)](https://codecov.io/gh/iqasport/referee_hub)

## Getting Started

It's highly recommended you use a ruby version manager during development of this app. The locked ruby version is '2.5.1'
and is required to develop on the application.


* System dependencies
  - ruby 2.5.1
  - bundler
  - postgresql
  - foreman

  - install ruby through either [RVM](https://rvm.io/) or [RBENV](https://github.com/rbenv/rbenv)
  - install postgresql through brew `brew install postgresql`
  - install foreman - `gem install foreman`

* Configuration
  - After you've cloned the repo and installed ruby, run `bundle install` in the root folder `referee_hub/`.
  - To run the application enter `foreman start -f Procfile.dev -p 3000` in your terminal.

* Database creation
  - To get the backend running you'll need to run `rails db:setup`

* How to run the test suite
  - Backend specs can be run like: `be rspec spec/models/referee_spec.rb`
    - You can also run a subset specs by including a line number like: `be rspec spec/models/referee_spec.rb:37`

* Deployment instructions
  More to come here soon.
