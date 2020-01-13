const { environment } = require('@rails/webpacker')

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

module.exports = environment
