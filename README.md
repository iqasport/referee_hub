# README
[![CircleCI](https://circleci.com/gh/iqasport/referee_hub/tree/master.svg?style=svg)](https://circleci.com/gh/iqasport/referee_hub/tree/master)
[![codecov](https://codecov.io/gh/iqasport/referee_hub/branch/master/graph/badge.svg)](https://codecov.io/gh/iqasport/referee_hub)

## Getting Started

It's highly recommended you use a ruby version manager during development of this app. The locked ruby version is '2.5.1'
and is required to develop on the application.


* System dependencies
  - ruby 2.5.1
  - bundler
  - yarn
  - postgresql
  - foreman

* Tips in case of missing system dependencies
  - install ruby through either [RVM](https://rvm.io/) or [RBENV](https://github.com/rbenv/rbenv)
  - install postgresql through brew `brew install postgresql`
  - install yarn `brew install yarn`
  - install foreman `gem install foreman`
  - install bundler `gem install bundler`

* Setup and running
  - After you've cloned the repo and installed ruby, run in the root folder `referee_hub/`:
    - `bundle install` to install ruby gem dependencies
    - `yarn install` to install javascript dependencies
    - `rails db:setup` to create the backend database (postgresql must be running)
  - To run the application enter `foreman start -f Procfile.dev -p 3000` in your terminal.

* How to run the test suite
  - Backend specs can be run like: `be rspec spec/models/referee_spec.rb`
    - You can also run a subset specs by including a line number like: `be rspec spec/models/referee_spec.rb:37`

* Deployment instructions
  More to come here soon.
