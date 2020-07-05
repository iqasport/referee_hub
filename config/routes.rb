# frozen_string_literal: true
require 'sidekiq/web'
require 'sidekiq/cron/web'
require 'flipper-ui'
require 'flipper-api'

Rails.application.routes.draw do
  # For details on the DSL available within this file, see https://guides.rubyonrails.org/routing.html
  mount PolicyManager::Engine => '/policies'
  mount Flipper::Api.app(Flipper) => '/flipper/api'

  authenticate :user, (proc { |user| user.iqa_admin? }) do
    mount Sidekiq::Web => '/sidekiq'
    mount Flipper::UI.app(Flipper) => '/flipper'
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
  get 'national_governing_bodies/:id', to: 'home#index'
  get 'import/:scope', to: 'home#index'

  devise_scope :user do
    get '/sign_up' => 'users/registrations#new', as: :user_registration
    get '/sign_in' => 'users/sessions#new', as: :new_user_session
    post '/sign_in' => 'users/sessions#create', as: :user_session
    delete '/sign_out' => 'users/sessions#destroy', as: :destroy_user_session
    get '/password' => 'users/passwords#new', as: :new_user_password
    get '/password/edit' => 'users/passwords#edit', as: :edit_user_password
    patch '/password' => 'users/passwords#update', as: :update_user_password
    post '/password' => 'users/passwords#create', as: :create_user_password
    get '/register/cancel' => 'users/registrations#cancel', as: :cancel_user_registration
    patch '/register' => 'users/registrations#update', as: :update_user_registration
    delete '/register' => 'users/registrations#destroy', as: :destroy_user_registration
    post '/register' => 'users/registrations#create', as: :create_user_registration
    get '/confirmation/new' => 'users/confirmations#new', as: :new_user_confirmation
    get '/confirmation' => 'users/confirmations#show', as: :user_confirmation
    post '/confirmation' => 'users/confirmations#create'
    get '/invitation' => 'users/invitations#new', as: :new_user_invitation
    post '/invitation' => 'users/invitations#create', as: :user_invitation
    get '/invitation/accept' => 'users/invitations#edit', as: :accept_user_invitation
    put '/invitation' => 'users/invitations#update', as: :update_user_invitation
  end

  devise_for :users, skip: :all

  namespace :api do
    namespace :v1 do
      resources :teams, only: :index
      resources :national_governing_bodies, only: %i[index show update] do
        member do
          post :update_logo
        end
      end
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
      resources :certifications, only: :index

      scope '/admin' do
        post '/search' => 'diagnostic#search'
        patch '/update-payment' => 'diagnostic#update_payment'
      end

      scope '/ngb-admin' do
        resources :teams,
                  only: %i[index show create update destroy],
                  controller: 'national_governing_body_teams'

        post 'teams_import', to: 'national_governing_body_teams#import'
        get 'teams_export', to: 'national_governing_body_teams#export'
      end

      post 'tests/import', to: 'tests#import'
      get 'referees' => 'referees#index'
      get 'referees/:id' => 'referees#show'
      put 'referees/:id' => 'referees#update'
      patch 'referees/:id' => 'referees#update'
      get 'referees_export' => 'referees#export'
      get 'users/current_user' => 'users#get_current_user'
      post 'users/:id/accept_policies' => 'users#accept_policies'
      post 'users/:id/reject_policies' => 'users#reject_policies'
      post 'users/:id/update_avatar' => 'users#update_avatar'
    end
  end

  root to: 'home#index'
end
