# Testing

## Automated tests
The backend has unit tests runnable with `dotnet test`.

The frontend has unit tests runnable with `yarn test`.

## Manual tests
You can run the service with `dotnet run` in the folder `src/backend/ManagementHub.Service`.
This will launch the service in Development mode on the port 5000.
In Development the dependencies are all in memory.

You can also publish the service into a docker image (see [building](./building.md)) and test it in an environment with real dependencies.
Run `docker compose up -d` in the folder `docker/staging` to setup a local cluster with Postgres, Redis and MailHog.
The service is available at port 80.
