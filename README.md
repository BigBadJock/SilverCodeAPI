# SilverCodeAPI
 
This project produces to NuGet packages with interfaces and abstract base classes for an API service. The base classes provide an abstract Generic Repository class that uses the REST-Parser (https://github.com/BigBadJock/REST-Parser), to provide a fully featured search, for example:

GET /people?surname[eq]=Smith&hometown[eq]=Edinburgh&sort_by[asc]=surname&sort_by[desc]=firstname&$page=1&$pageSize=10
