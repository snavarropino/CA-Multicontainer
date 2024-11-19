# Rating API

Sample API for get an manage an album ratting, built in .NET 8

## Endpoints

### GET /

Can be used to check if the service is up and running.

### GET /rating/albumId

Get album rating by albumId (ramdom value between 1 and 5)

### POST /rating/albumId

Simulates that ratijng is updated returning Ok

## Build and run locally

```cmd
dotnet run
```

The API listen on port 5080

## Buld and run in Docker

From repository root execute

```cmd
cd ratingapi
docker build -t ratingapi .
docker run -p 5080:5080 ratingapi
```

Then you can reach the api at http://localhost:5080/



