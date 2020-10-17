[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=k-boyle_Espeon&metric=ncloc)](https://sonarcloud.io/dashboard?id=k-boyle_Espeon) [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=k-boyle_Espeon&metric=sqale_rating)](https://sonarcloud.io/dashboard?id=k-boyle_Espeon)
 
V4 branch is a WIP, look at v3 for older (questionable) source

**Example Config**
```json
{
  "Discord": {
    "Token": "NDU3NTA3NTUxNzI3MDU4OTQ2.XpDsgA.BItzNqFuaYvVDZ9hisBV4F_gj8A"
  },
  "Postgres": {
    "ConnectionString": "Host=127.0.0.1;Port=5432;Database=Espeon;Username=postgres;Password=casino"
  },
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File",
      "Serilog.Extensions.Logging",
      "Espeon"
    ],
    "Enrich": [
      "WithClassName"
    ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft.EntityFrameworkCore": "Warning",
        "Microsoft.Extensions.Hosting": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Espeon.EspeonLoggingConsoleTheme::Instance, Espeon",
          "outputTemplate": "{Timestamp: HH:mm:ss} | {Level,-15} | {ClassName} | {Message}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "./logs/log-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:dd-MM-yyyy HH:mm:ss} | {Level} | {SourceContext} | {Message}{NewLine}{Exception}"
        }
      }
    ]
  },
  "Localisation": {
    "Path": "./Localisation/"
  }
}
```

**Running From Source**
```
git clone git@github.com:k-boyle/Espeon.git
cd Espeon/src
dotnet publish -c release -o ./output
cd /output
dotnet Espeon.dll
```

**Running From Docker**
