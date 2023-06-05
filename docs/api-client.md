# API Client
The backend service exposes its API via Swagger.
You can view all the API endpoints by visiting Swagger UI at `/swagger/index.html`.

A generate RTK Query client is used to make API calls and take care of data caching.
To regenerate the client launch the service with `dotnet run` and then execute `yarn swaggergen`.
The configuration can be found in `openapi-config.js`.

Docs: https://redux-toolkit.js.org/rtk-query/usage/code-generation
