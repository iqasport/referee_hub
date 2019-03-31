class QuestionSerializer
  include FastJsonapi::ObjectSerializer

  attributes :description, :feedback, :test_id, :points_available
end

