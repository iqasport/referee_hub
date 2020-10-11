# Preview all emails at http://localhost:3000/rails/mailers/user_mailer
class UserMailerPreview < ActionMailer::Preview
  def referee_answer_feedback_email
    started_at = Time.now.utc.to_s
    finished_at = (Time.now.utc + 15.minutes).to_s
    referee = User.create(
      first_name: 'Test',
      last_name: 'Testerton',
      email: "#{FFaker::Name.first_name}.tester@example.com",
      password: 'password'
    )
    referee.confirm_all_policies!
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

    UserMailer
      .with(referee: referee, test_attempt: test_attempt, test_result: test_result)
      .referee_answer_feedback_email
  end

  def export_csv_email
    user = User.create(
      first_name: 'Test',
      last_name: 'Testerton',
      email: "#{FFaker::Name.first_name}.tester@example.com",
      password: 'password'
    )
    user.confirm_all_policies!
    csv = ExportedCsv.create!(user_id: user.id, url: FFaker::Internet.http_url)

    UserMailer.with(user: user, csv: csv).export_csv_email
  end

  def recertification_failure_email
    started_at = Time.now.utc.to_s
    finished_at = (Time.now.utc + 15.minutes).to_s

    referee = User.create(
      first_name: 'Test',
      last_name: 'Testerton',
      email: "#{FFaker::Name.first_name}.tester@example.com",
      password: 'password'
    )
    referee.confirm_all_policies!

    test = Test.create!(
      level: 'snitch',
      name: 'Snitch Referee Test',
      negative_feedback: 'You did bad',
      positive_feedback: 'You did good',
      description: 'A snitch test',
      certification: Certification.find_by(level: 'snitch')
    )

    test_result = TestResult.create!(
      test: test,
      duration: '00:15:00',
      passed: false,
      minimum_pass_percentage: 50,
      percentage: 10,
      points_available: 100,
      points_scored: 10,
      time_finished: finished_at,
      time_started: started_at,
      referee: referee
    )

    UserMailer.with(referee: referee, test_result: test_result).recertification_failure_email
  end
end
