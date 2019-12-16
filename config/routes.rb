# frozen_string_literal: true
require 'sidekiq/web'

Rails.application.routes.draw do
  mount PolicyManager::Engine => "/policies"

  authenticate :user, proc { |user| user.iqa_admin? } do
    mount Sidekiq::Web => '/sidekiq'
  end

  get 'home/index'
  get 'referees', to: 'home#index'
  get 'referees/:id', to: 'home#index'
  get 'privacy', to: 'home#index'
  get 'admin', to: 'home#index'
  get 'admin/referee-diagnostic', to: 'home#index'
  get 'admin/tests', to: 'home#index'
  get 'admin/tests/:id', to: 'home#index'
  get '/referees/:referee_id/tests/:test_id', to: 'home#index'

  post 'webhook', to: 'classmarker#webhook'

  devise_scope :user do
    get '/sign_up' => 'devise/registrations#new', as: :user_registration
    get '/sign_in' => 'devise/sessions#new', as: :new_user_session
    post '/sign_in' => 'devise/sessions#create', as: :user_session
    delete '/sign_out' => 'devise/sessions#destroy', as: :destroy_user_session
    get '/password' => 'devise/passwords#new', as: :new_user_password
    get '/password/edit' => 'devise/passwords#edit', as: :edit_user_password
    patch '/password' => 'devise/passwords#update', as: :update_user_password
    post '/password' => 'devise/passwords#create', as: :create_user_password
    get '/register/cancel' => 'devise/registrations#cancel', as: :cancel_user_registration
    patch '/register' => 'devise/registrations#update', as: :update_user_registration
    delete '/register' => 'devise/registrations#destroy', as: :destroy_user_registration
    post '/register' => 'devise/registrations#create', as: :create_user_registration
    get '/confirmation/new' => 'devise/confirmations#new', as: :new_user_confirmation
    get '/confirmation' => 'devise/confirmations#show', as: :user_confirmation
    post '/confirmation' => 'devise/confirmations#create'
  end

  devise_for :users, skip: :all

  namespace :api do
    namespace :v1 do
      resources :users, only: %i[index show update]
      resources :national_governing_bodies, only: %i[index show]
      resources :referee_certifications, only: %i[index create update]
      resources :tests, only: %i[index create show update destroy] do
        resources :questions, shallow: true do
          resources :answers, shallow: true
        end

        member do
          get :start
          post :finish
        end
      end

      scope '/admin' do
        post '/search' => 'diagnostic#search'
        patch '/update-payment' => 'diagnostic#update_payment'
      end

      post 'tests/import', to: 'tests#import'
      get 'referees' => 'referees#index'
      get 'referees/:id' => 'referees#show'
      put 'referees/:id' => 'referees#update'
      patch 'referees/:id' => 'referees#update'
    end
  end


  root to: 'home#index'
end
