class MigrateLanguageToNewLanguage < ActiveRecord::Migration[6.0]
  def up
    Test.all.find_each do |t|
      current_language = t.language
      current_region = nil
      current_region = 'ES' if current_language =~ /Europa/
      current_region = '419' if current_language =~ /Latinoamérica/
      current_language = 'Español' if current_language =~ /Español/
      new_language = Language.find_by(long_name: current_language, short_region: current_region)
      new_language = Language.find_by(long_name: 'English') unless new_language.present?

      t.update(new_language_id: new_language.id)
    end
  end

  def down
    raise ActiveRecord::IrreversibleMigration
  end
end
