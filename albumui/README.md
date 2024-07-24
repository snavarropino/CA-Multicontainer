# Album Viewer UI

Sample album App UI built in Express

## Build and run locally

From project folder run

```cmd
npm install
npm start
```

Navigate to http://localhost:3000/ 

## Buld and run in Docker

From repository root execute

```cmd
cd albumui
docker build -t albumui .
docker run -p 3000:3000 albumui

Navigate to http://localhost:3000/ 

> [!IMPORTANT]  
> Execute rating and album api's before executing ui