# frozen_string_literal: true

Rails.application.routes.draw do
  namespace :api do
    namespace :v1 do
      resources :referees, only: %i[index show update]
      resources :national_governing_bodies, only: %i[index show]
      resources :referee_certifications, only: %i[index update]
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
    end
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

  devise_scope :referee do
    get '/sign_up' => 'devise/registrations#new', as: :referee_registration
    get '/sign_in' => 'devise/sessions#new', as: :new_referee_session
    post '/sign_in' => 'devise/sessions#create', as: :referee_session
    delete '/sign_out' => 'devise/sessions#destroy', as: :destroy_referee_session
    get '/password' => 'devise/passwords#new', as: :new_referee_password
    get '/password/edit' => 'devise/passwords#edit', as: :edit_referee_password
    patch '/password' => 'devise/passwords#update', as: :update_referee_password
    post '/password' => 'devise/passwords#create', as: :create_referee_password
    get 'register/cancel' => 'devise/registrations#cancel', as: :cancel_referee_registration
    patch '/register' => 'devise/registrations#update', as: :update_referee_registration
    delete '/register' => 'devise/registrations#destroy', as: :destroy_referee_registration
    post '/register' => 'devise/registrations#create', as: :create_referee_registration
  end

  devise_for :referees, skip: :all

  root to: 'home#index'
end
