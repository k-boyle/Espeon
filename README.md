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
  "Logging": {
    "WriteToFile": true,
    "WriteToConsole": true,
    "Path": "./logs/",
    "Level": "Information",
    "RollingInterval": "Day" 
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
