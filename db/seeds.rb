NGB_CONFIG = [
  {
    name: 'Argentina',
    website: 'https://www.facebook.com/asociaciondequidditch.arg/',
  },
  {
    name: 'Australia',
    website: 'http://www.quidditch.org.au/',
  },
  {
    name: 'Austria',
    website: 'https://quidditch.at/en/',
  },
  {
    name: 'Belgium',
    website: 'https://www.quidditchbelgium.be/',
  },
  {
    name: 'Brazil',
    website: 'https://abrquadribol.wordpress.com/',
  },
  {
    name: 'Canada',
    website: 'http://quidditchcanada.com/',
  },
  {
    name: 'Catalonia',
    website: 'https://www.facebook.com/QuidditchCatalunya/',
  },
  {
    name: 'Chile',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Czech Republic',
    website: 'http://famfrpal.cz/',
  },
  {
    name: 'Denmark',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'France',
    website: 'http://www.quidditch.fr/',
  },
  {
    name: 'Germany',
    website: 'http://www.deutscherquidditchbund.de/index.php/en/',
  },
  {
    name: 'Hungary',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Iceland',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'India',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Indonesia',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Ireland',
    website: 'https://www.facebook.com/QuidditchIreland/',
  },
  {
    name: 'Israel',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Italy',
    website: 'http://www.italiaquidditch.com/index.php/it/',
  },
  {
    name: 'Japan',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Mexico',
    website: 'https://www.qmxsport.org/',
  },
  {
    name: 'Netherlands',
    website: 'http://www.quidditchnederland.nl/nl/',
  },
  {
    name: 'New Zealand',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Norway',
    website: 'http://rumpeldunk.no/en/971-2/',
  },
  {
    name: 'Pakistan',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Peru',
    website: 'https://www.facebook.com/QuidditchPeru/',
  },
  {
    name: 'Poland',
    website: 'https://www.facebook.com/PolskaLigaQuidditcha',
  },
  {
    name: 'Portugal',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Republic of Korea',
    website: 'https://www.facebook.com/quidditchkorea/',
  },
  {
    name: 'Romania',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Serbia',
    website: 'http://www.iqasport.com/membership/member-ngbs',
  },
  {
    name: 'Slovakia',
    website: 'https://www.facebook.com/slovakquidditch/',
  },
  {
    name: 'Slovenia',
    website: 'https://www.facebook.com/quidditch.ljubljana/',
  },
  {
    name: 'Spain',
    website: 'https://www.facebook.com/AsociacionQuidditchEspana/',
  },
  {
    name: 'Sweden',
    website: 'https://www.facebook.com/QuidditchSverige/',
  },
  {
    name: 'Switzerland',
    website: 'http://www.swissquidditch.ch/',
  },
  {
    name: 'Turkey',
    website: 'http://quidditchdernegi.org/',
  },
  {
    name: 'Uganda',
    website: 'https://www.facebook.com/Quidditch-Uganda-1451741015090784/',
  },
  {
    name: 'United Kingdom',
    website: 'https://www.quidditchuk.org/',
  },
  {
    name: 'United States',
    website: 'https://www.usquidditch.org/',
  },
].freeze

NGB_CONFIG.each do |config|
  NationalGoverningBody.find_or_create_by(name: config[:name], website: config[:website])
end

CERT_CONFIG = [
  {
    display_name: 'Snitch Referee Certification',
    level: :snitch
  },
  {
    display_name: 'Assistant Referee Certification',
    level: :assistant
  },
  {
    display_name: 'Head Referee Certification',
    level: :head
  },
  {
    display_name: 'Field Test Certification',
    level: :field
  },
].freeze

CERT_CONFIG.each do |config|
  Certification.find_or_create_by(display_name: config[:display_name], level: config[:level])
end

USER_ROLES = %w[referee iqa_admin ngb_admin]
USER_ROLES.each do |access_type|
  FactoryBot.create(:user, password: 'password', roles_attributes: [{ access_type: access_type }])
end
