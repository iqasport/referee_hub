# == Schema Information
#
# Table name: questions
#
#  id               :bigint(8)        not null, primary key
#  description      :text             not null
#  feedback         :text
#  points_available :integer          default(1), not null
#  created_at       :datetime         not null
#  updated_at       :datetime         not null
#  test_id          :integer          not null
#

class QuestionSerializer
  include FastJsonapi::ObjectSerializer

  attributes :description, :feedback, :test_id, :points_available

  has_many :answers, serializer: :answer
end
