class AddCertificationIdToTests < ActiveRecord::Migration[6.0]
  def up
    tests = Test.where(certification_id: nil)
    snitch = Certification.find_by(level: 'snitch')
    assistant = Certification.find_by(level: 'assistant')
    head = Certification.find_by(level: 'head')

    tests.each do |test|
      cert_id = nil
      if test.level == 'snitch'
        cert_id = snitch.id
      elsif test.level == 'assistant'
        cert_id = assistant.id
      elsif test.level == 'head'
        cert_id = head.id
      end

      test.update!(certification_id: cert_id)
    end
  end

  def down
    raise ActiveRecord::IrreversibleMigration
  end
end
