# Album API

Sample API for get an album list, built in .NET 6

## Endpoints

### GET /

Can be used to check if the service is up and running.

### GET /albums

Get album list. It uses other service to get rating for each album. See ratingapi for more information.

## Build and run locally

```cmd
dotnet run
```

The API listen on port 8080

## Buld and run in Docker

From repository root execute

```cmd
cd albumapi
docker build -t albumapi .
docker run -p 8080:8080 albumapi
```

Then you can reach the api at http://localhost:8080/

> [!IMPORTANT]  
> Execute rating api before album api