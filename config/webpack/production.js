process.env.NODE_ENV = process.env.NODE_ENV || "production";

const environment = require("./environment");

environment.loaders.get("babel").exclude = [
  /\.test\.(ts|tsx)$/,
  environment.loaders.get("babel").exclude,
];

module.exports = environment.toWebpackConfig();
