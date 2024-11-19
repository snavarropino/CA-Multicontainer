# Album application sample

Pulumi IaC that deploys a sample application that deploys a simple album API and a rating API, as well a an UI that consumes both APIs.

## Deploy

From infrastructure folder execute:
```cmd
az login
az account set -s <subscriptionId>
pulumi config set azure-native:location <location>
pulumi stack init <stackname>
pulumi up
```

Navigate to albumui ingress url to see the application running.

## Remove resources

From infrastructure folder execute:
```cmd
pulumi destroy
```