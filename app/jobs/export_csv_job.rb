class ExportCsvJob < ApplicationJob
  queue_as :mailers
  rescue_from ActiveJob::DeserializationError do |exception|
    retry_job wait: 5.minutes unless exception.message.include? "Couldn't find"
  end

  def perform(user, type, export_options)
    klass = constantize(type)
    klass.create!(user: user, export_options: export_options)
  end
end
