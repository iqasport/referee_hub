# frozen_string_literal: true

Rails.application.routes.draw do
  get 'home/index'
  devise_for :referees, controllers: {
    sessions: 'referees/sessions',
    passwords: 'referees/passwords',
    registrations: 'referees/registrations'
  }

  root to: 'home#index'
end
