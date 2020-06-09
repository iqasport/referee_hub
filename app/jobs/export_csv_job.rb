require 'active_support/inflector'

class ExportCsvJob < ApplicationJob
  queue_as :mailers
  rescue_from ActiveJob::DeserializationError do |exception|
    retry_job wait: 5.minutes unless exception.message.include? "Couldn't find"
  end

  def perform(user, export_type, export_options)
    klass = ActiveSupport::Inflector.constantize(export_type)
    klass.create!(user: user, export_options: export_options)
  end
end
