## Dockerfile.dev
In order to simplify local deployment I've created a docker-compose setup with a Dockerfile for creating a custom image with the application.

* The contents of the repository is copied to the `/code` folder, which will be the working root of the container.
* Ruby and Node dependencies are stored in docker managed volumes, so that they are persisted across app container changes. You need at least a 1GB of free disk space for those.
    * It's important for the volumes to be docker managed so that they live on a ext4 partition - when using WSL2 with Windows volumes everything slows down. 
* If you want to update packages, instead of copying `/code` you want to mount it as a volume (modify `Dockerfile` and `docker-compose.yml` - see comments inside).
* On the first couple runs uncomment the appropriate `CMD` for installing dependencies and configuring database (one at a time).
* Application should be controlled using the `ENV` declared variables.

```
# start containers
docker compose up -d --build
# watch application logs
docker compose logs -f dev
```

You should be able to access the application at `http://localhost:3000`.
