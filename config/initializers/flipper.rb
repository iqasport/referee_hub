require 'flipper'
require 'flipper/adapters/active_record'

Flipper.configure do |config|
  config.default do
    adapter = Flipper::Adapters::ActiveRecord.new
    Flipper.new(adapter)
  end
end

Flipper.register(:iqa_admins) do |actor|
  actor.respond_to?(:iqa_admin?) && actor.iqa_admin?
end

Flipper.register(:ngb_admins) do |actor|
  actor.respond_to?(:ngb_admin?) && actor.ngb_admin?
end
