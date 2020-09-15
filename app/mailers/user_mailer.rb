class UserMailer < ApplicationMailer
  default from: 'noreply@iqareferees.org'

  def referee_answer_feedback_email
    @test_attempt = params[:test_attempt]
    @test_result = params[:test_result]
    @referee = params[:referee]
    @test = @test_attempt.test
    @next_attempt_at = @test_attempt.next_attempt_at.to_time
    @url = "https://manage.iqasport.com/referees/#{@referee.id}"

    mail(to: @referee.email, subject: "#{@test.name} Results")
  end

  def export_csv_email
    @user = params[:user]
    @exported_csv = params[:csv]
    export_type = ExportedCsv.format_type(@exported_csv.type)

    mail(to: @user.email, subject: "Your #{export_type} is ready")
  end
end
