Rails.application.routes.draw do
  get 'home/index'
  devise_for :referees

  root to: 'home#index'
end
