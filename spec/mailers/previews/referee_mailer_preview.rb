# Preview all emails at http://localhost:3000/rails/mailers/referee_mailer
class RefereeMailerPreview < ActionMailer::Preview
  def referee_answer_feedback_email
    started_at = Time.now.utc.to_s
    finished_at = (Time.now.utc + 15.minutes).to_s
    referee = Referee.create!(
      first_name: 'Test',
      last_name: 'Testerton',
      email: "#{FFaker::Name.first_name}.tester@example.com",
      password: 'password'
    )

    test_attrs = {
      level: 0,
      name: '2018-2020 Snitch Referee Test',
      negative_feedback: 'You did bad',
      positive_feedback: 'You did good',
      description: 'A snitch test'
    }
    test = Test.create!(test_attrs)

    question_attrs = { test: test, description: 'This is a question', feedback: '<div>Do better next time</div>' }
    3.times do
      Question.create!(question_attrs)
    end

    questions = test.questions

    questions.each do |question|
      answer_attrs = { question: question, description: 'I am an answer' }
      4.times do |num|
        answer_attrs[:correct] = true if num == 3
        Answer.create!(answer_attrs)
      end
    end
    referee_answers = questions.map do |question|
      answer = question.randomize_answers.first
      { question_id: question.id, answer_id: answer.id }
    end

    test_result = Services::GradeFinishedTest.new(
      test: test,
      referee: referee,
      started_at: started_at,
      finished_at: finished_at,
      referee_answers: referee_answers,
      skip_email: true
    ).perform
    test_attempt = referee.test_attempts.last

    RefereeMailer
      .with(referee: referee, test_attempt: test_attempt, test_result: test_result)
      .referee_answer_feedback_email
  end
end
