const { environment } = require('@rails/webpacker')
const typescript = require('./loaders/typescript')

environment.splitChunks()

environment.config.merge({
  module: {
    rules: [
      {
        test: /\.js$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader'
        }
      }
    ]
  }
})

environment.loaders.prepend('typescript', typescript)

module.exports = environment
