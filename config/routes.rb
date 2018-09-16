# frozen_string_literal: true

Rails.application.routes.draw do
  namespace :api do
    namespace :v1 do
      resources :referees, only: %i[index show update]
    end
  end

  get 'home/index'
  get 'referees', to: 'home#index'
  get 'referees/:id', to: 'home#index'

  post 'webhook', to: 'classmarker#webhook'

  devise_for :referees, controllers: {
    sessions: 'referees/sessions',
    passwords: 'referees/passwords',
    registrations: 'referees/registrations'
  }

  root to: 'home#index'
end
