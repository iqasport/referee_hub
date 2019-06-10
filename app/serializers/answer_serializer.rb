class AnswerSerializer
  include FastJsonapi::ObjectSerializer

  attributes :description, :correct, :question_id
end
