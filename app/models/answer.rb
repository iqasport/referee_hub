# == Schema Information
#
# Table name: answers
#
#  id          :bigint(8)        not null, primary key
#  correct     :boolean          default(FALSE), not null
#  description :text             not null
#  created_at  :datetime         not null
#  updated_at  :datetime         not null
#  question_id :integer          not null
#

class Answer < ApplicationRecord
  belongs_to :question
  has_many :referee_answers, dependent: :destroy
end
