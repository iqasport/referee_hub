# README

[![CircleCI](https://circleci.com/gh/iqasport/referee_hub/tree/master.svg?style=svg)](https://circleci.com/gh/iqasport/referee_hub/tree/master)
[![codecov](https://codecov.io/gh/iqasport/referee_hub/branch/master/graph/badge.svg)](https://codecov.io/gh/iqasport/referee_hub)
[![Heroku](https://heroku-badge.herokuapp.com/?app=referee-hub)](https://heroku-badge.herokuapp.com/?app=referee-hub)
[![CodeFactor](https://www.codefactor.io/repository/github/iqasport/referee_hub/badge)](https://www.codefactor.io/repository/github/iqasport/referee_hub)

## Getting Started

If you want to use docker for development purposes instead of installing tools and services manually, see `docker/README.md`.

---

It's highly recommended you use a ruby version manager during development of this app. The locked ruby version is '2.7.1'
and is required to develop on the application.

- System dependencies

  - ruby 2.7.1
  - bundler
  - yarn
  - postgresql
  - foreman
  - redis

- Tips in case of missing system dependencies

  - install ruby through either [RVM](https://rvm.io/) or [RBENV](https://github.com/rbenv/rbenv)
  - install postgresql through brew `brew install postgresql`
  - install redis through brew `brew install redis`
  - install yarn `brew install yarn`
  - install foreman `gem install foreman`
  - install bundler `gem install bundler`

- Setup and running

  - After you've cloned the repo and installed ruby, run in the root folder `referee_hub/`:
    - `bundle install` to install ruby gem dependencies
    - `yarn install` to install javascript dependencies
    - `rails db:setup` to create the backend database (postgresql must be running)
  - To run the application enter `./bin/start` from the root folder. If you're having permission issues running the script make sure you
    ensure the script is executable by running `chmod +x ./bin/start`

* How to run the test suite

  - Backend specs can be run like: `be rspec spec/models/referee_spec.rb`
    - You can also run a subset specs by including a line number like: `be rspec spec/models/referee_spec.rb:37`

* Contributing to the codebase

  - Create a branch off of master detailing the work you'll be doing. `git checkout -b testing-framework-backend`
  - Break up your work with small, iterative, commits that describe the changes that have been made.
    A typical branch could look like:
    ```
      - @AfroDevGirl Adds referee_answer model to store a referee's answers relative to a test ce1a18b
      - @AfroDevGirl Adds testable question count to test so questions can be randomly selected 46e4dcd
      - @AfroDevGirl Adds test starting functionality to api 3751d76
      - @AfroDevGirl Adds test finish functionality 62a4632
      - @AfroDevGirl Adds test result mailer and fixes webpack version issue 8278148
      - @AfroDevGirl Adds validation for tests in cool down period 45c2dab
      - @AfroDevGirl Fixes spec running 7ab56ca
    ```
  - Push up your branch by running `git push -u origin testing-framework-backend` and create a new Pull Request.
    Be as descriptive as possible in the PR comment about the work you've done.
  - After all checks have passed your code will be able to be merged

* Deployment instructions
  - Deploys are automatically managed through Heroku after a branch is successfully merged.
    Changes to the deploy process should be made in `Procfile`

-----

## CSS
CSS is compiled using PostCSS and Tailwind. See configuration in `postcss.config.js` which imports packages defined in `package.json`.

Compiled CSS is included in the core application for single pages (like auth) and for the SPA.

We can use AWS CloudFront to be a CDN on top of our application. The service hosts static files such as CSS/JS in a normal way.
We have a CF endpoint pointing at our service. We use the CF endpoint when referring to our static assets so that they're cached and geo distributed.

In rails this was done automatically using these instructions https://devcenter.heroku.com/articles/using-amazon-cloudfront-cdn#adding-cloudfront-to-rails
and by calling special tags in views.
