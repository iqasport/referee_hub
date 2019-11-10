process.env.NODE_ENV = process.env.NODE_ENV || 'production'
const { BugsnagBuildReporterPlugin, BugsnagSourceMapUploaderPlugin } = require('webpack-bugsnag-plugins')
const environment = require('./environment')

const apiKey = process.env.RAILS_ENV !== 'production' ? 'iamAFak3apiKey' : process.env.BUGSNAG_API_KEY
const buildReporter = new BugsnagBuildReporterPlugin({
  apiKey,
  sourceControl: {
    provider: 'github',
    repository: 'https://github.com/iqasport/referee_hub'
  },
  autoAssignRelease: true
})
const sourceMap = new BugsnagSourceMapUploaderPlugin({
  apiKey
})

environment.plugins.prepend('BugsnagBuildReporterPlugin', buildReporter)
environment.plugins.prepend('BugsnagSourceMapUploaderPlugin', sourceMap)

module.exports = environment.toWebpackConfig()
