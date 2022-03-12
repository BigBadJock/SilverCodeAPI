# SilverCodeAPI
 
This project produces to NuGet packages with interfaces and abstract base classes for an API service. The base classes provide an abstract Generic Repository class that uses the REST-Parser (https://github.com/BigBadJock/REST-Parser), to provide a fully featured search, for example:

GET /people?surname[eq]=Smith&hometown[eq]=Edinburgh&sort_by[asc]=surname&sort_by[desc]=firstname&$page=1&$pageSize=10

update readme to test github actions



# Using GitHub packages

1. Get a token from Github/Settings/Developer Settings/Personal Access Tokens/
1. Run nuget setApiKey <accesstoken> -source github
1. Add nuget.Config - ensure that nuget.config is added to .gitIgnore

```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json"/>
    <add key="github" value="https://nuget.pkg.github.com/bigbadjock/index.json"/>
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="UserName" value="bigbadjock"/>
      <add key="ClearTextPassword" value="<accessToken>"/>
    </github>
  </packageSourceCredentials>
</configuration>
```