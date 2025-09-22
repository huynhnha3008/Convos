# FAMS - APIGATEWAY



## Getting started

This project is developed using .NET 8.0, with primary responsibility of routing requests to specific service endpoints in microservices architecture. Additionally, It's allowed seamless integration with IdentityServer for user authentication and authorization

- [ ] [Ocelot Configuration](https://ocelot.readthedocs.io/en/latest/features/configuration.html)


--- 


## Overview - APIGateway (Ocelot)

- Ocelot play a role as APIGateway within a microservices architecture responsible for unified point of entry into system. It will work with either HTTP or HTTPS and run on any platform.
- Particularly, It easy integration with IdentityServer (more detail in [~/Backend/IdentityAPI/README.md]) and Bearer tokens. It's allowed to seeking current workplace.
- Ocelot is an open-source APIGateway built on .NET platform, providing features such as routing, request aggregation, load balancing, authentication and authorization
- With Ocelot, It's easier to manage and secure microservices architecture

- [ ] [BigPicture](https://ocelot.readthedocs.io/en/latest/introduction/bigpicture.html)


---


## APIGateway Dependencies

Foundation Package References:
    + Microsoft.VisualStudio.Azure.Containers.Tools.Targets
    + Microsoft.AspNetCore.Authentication.JwtBearer
    + Ocelot

---

## ocelot.json 

- This file serves as a central configuration file for Ocelot to meet the specfic
endpoint of services

- [ ] [GettingStarted](https://ocelot.readthedocs.io/en/latest/introduction/gettingstarted.html)


- Base configuration

```
    "Routes": [],
    "GlobalConfiguration": {
        "BaseUrl": "https://api.mybusiness.com"
    }
```

"Routes": Route definition how incoming requests are mapped and routed to downstream microservices or endpoints. Including details such as route templates, downstream host addresses, and optional path manipulation

"GlobalConfiguration": Global settings for Ocelot, such as base URL, service provider (auth provider), and load balancer configuration (--optional)




- Default template: Please put this inside "Routes"

```
    {
        "DownstreamPathTemplate": "/api/[API endpoints here...]",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [
        {
            "Host": "localhost",
            "Port": <Check in [~/Properties/lauchSettings.json]>
        }
        ],
        "UpstreamPathTemplate": "/api/[API endpoints here...]",
        "UpstreamHttpMethod": ["GET", "POST", "DELETE"],
        "AuthenticationOptions": {
            "AuthenticationProviderKey": "Bearer",
            "AllowedScopes": ["FamsApp", "openid", "profile"]
        }
    }
```


Port must be different from other services
If you want to define your service port, please customise [profiles] > [http] > [applicationUrl] in lauchSettings.json  
Docker port: <trainingprogram-svc: ports: 7001:80>. External/Listening port is 7001, please customise later in docker-compose.yml
All services port will be defined after unified with mentor later. You can defined our own port if running with [Development] environment. For those who want to dockerize your services with SQL image in Docker, please write Dockerfile and define your port in [docker-compose.yml] file.


---


- EXAMPLE:

I want to map API endpoints for [TrainingProgramManagementAPI] - [GET,POST,PUT,DELETE...]
with authentication. Please put this inside "Routes"

```
    {
        "DownstreamPathTemplate": "/api/trainingprograms",
        "DownstreamScheme": "http",
        "DownstreamHostAndPorts": [
            {
            "Host": "localhost",
            "Port": 7001
            }
        ],
        "UpstreamPathTemplate": "/api/trainingprograms",
        "UpstreamHttpMethod": ["GET", "POST", "PUT", "DELETE"],
        "AuthenticationOptions": {
            "AuthenticationProviderKey": "Bearer",
            "AllowedScopes": ["FamsApp", "openid", "profile"]
        }
    }

```

Map API enpoints for [TrainingProgramManagementAPI] - [GET] with paramters, authentication

```
    {
      "DownstreamPathTemplate": "/api/trainingprograms/{params}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 7001
        }
      ],
      "UpstreamPathTemplate": "/api/trainingprograms/{params}",
      "UpstreamHttpMethod": ["GET"],
      "AuthenticationOptions": {
        "AuthenticationProviderKey": "Bearer",
        "AllowedScopes": ["FamsApp", "openid", "profile"]
      }
    }
```

Please read Bug Issues at Line 189


--- 


## Get JWT token or Register to get access permission 
    - [~/Backend/IdentityAPI/README.md] 
    - Postman: [Line 61 and 152]
    - From FE: [Line 133 and 184]


--- 


## Executing Directives

LOCAL DEVELOPMENT: 

1. Open Command Line Interface (CLI) - VSCode Terminal, PowerShell...

2. Navigate to Project Directory 
    - [cd][./Backend/APIGateway]

3. Execute with `dotnet run`, this command will build and run the project. Ensure that all necessary dependencies are installed
    - [dotnet run] 

3.1. Excute with `dotnet watch` (Optional -for automatic rebuilds)
    - [dotnet watch]

---

DOCKER: 

1. Open Command Line Interface (CLI) - VSCode Terminal, PowerShell...

2. Navigate to Root Project Directory 
    - [cd][~/your.directory/fam_hcm24_cpl_net_02]

3. Build container
    - [docker-compose build][container-name]

4. Execute Docker Compose: 
    - [docker-compose up] or [docker-compose up -d] with detached mode

5. Stop and remove containers:
    - [docker-compose down]


---

## Bug Issues: 
    - Cannot read routing configuration from oclet.json except for appsettings.{HostingEnvironment}.json 
    - I recommend config your own API endpoints in appsettings.Development.json for Development and appsettings.Docker.json for Docker
    - I'll handle this later, sorry for this... 

