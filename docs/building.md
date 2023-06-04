# Building

## Frontend
The frontend project is a React application written in TypeScript and managed with Yarn.

You can build the frontend project on its own by invoking `yarn build:dev` or `yarn build:prod`
which executes `yarn build:app:*` a webpack to compile and bundle the project into a set of JS files in the `dist` folder.

In addition to the JS files, `package.json` describes compilation of two CSS files with `yarn styles`.
The styles compilation needs to be invoked after webpack, because the JS build cleans the folder.

### Webpack output
The JS files in the output folder have a hash appended to the file name - this is so that when a new version of the
website is deployed, upon refreshing the page new scripts will be downloading straight away - otherwise the browser may cache
them and if various caches expire at different times it could lead to a broken state for the user.

## Backend
The backend project is a .NET Core application written in C#.

You can build the backend solution by invoking `dotnet build`.
The main entry point is the `ManagementHub.Service` project which will also automatically
build the frontend and include it in its output.

### Publishing a Docker image
The backend with the frontend can be packaged together into a standalone Docker image.
Simply invoke `dotnet publish --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer`.

Additionally the container registry can be specified `-p:ContainerRegistry=<repository>`
for AWS ECR (public image). You can find the `iqasport/management-hub` repository details
in the AWS console.
