class CreateLanguages < ActiveRecord::Migration[6.0]
  def up
    # create hash of language key to hash of regions, short_name value
    language_hash = {
      'English' => {
        short_name: 'en',
        regions: ['GB', 'US']
      },
      'Português' => {
        short_name: 'pt',
        regions: ['BR']
      },
      'Español' => {
        short_name: 'es',
        regions: ['ES', '419']
      },
      'Français' => {
        short_name: 'fr'
      },
      'Deutsche' => {
        short_name: 'de'
      },
      'Italiano' => {
        short_name: 'it'
      },
      'Català' => {
        short_name: 'ca'
      },
      'Nederlands' => {
        short_name: 'nl',
        regions: ['BE']
      },
      'Türk' => {
        short_name: 'tr'
      },
      'Tiếng Việt' => {
        short_name: 'vi'
      },
      '日本語' => {
        short_name: 'ja'
      },
      '中文' => {
        short_name: 'zh',
        regions: ['CN']
      }
    }

    language_hash.each do |language, language_data|
      if language_data[:regions].present?
        language_data[:regions].each do |region|
          Language.create!(
            long_name: language,
            short_name: language_data[:short_name],
            short_region: region
          )
        end
      else
        Language.create!(
          long_name: language,
          short_name: language_data[:short_name]
        )
      end
    end
  end

  def down
    Language.destroy_all!
  end
end
