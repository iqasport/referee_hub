# frozen_string_literal: true

Rails.application.routes.draw do
  get 'home/index'
  devise_for :referees, controllers: {
    sessions: 'referees/sessions'
  }

  root to: 'home#index'
end
