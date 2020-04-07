class CustomFailureApp < Devise::FailureApp
  def route(_scope)
    "/"
  end
end