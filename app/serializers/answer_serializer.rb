# == Schema Information
#
# Table name: answers
#
#  id          :bigint           not null, primary key
#  correct     :boolean          default(FALSE), not null
#  description :text             not null
#  created_at  :datetime         not null
#  updated_at  :datetime         not null
#  question_id :integer          not null
#

class AnswerSerializer < BaseSerializer
  attributes :description, :question_id
end
