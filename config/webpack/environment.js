const { environment } = require('@rails/webpacker')

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
