# Solution
To run the solution you should have an instance of a MySQL Database running and add
the following configuration to your __appsettings.json__ file.
```json
"ConnectionStrings": {
    "DefaultConnection": "Server=your-server;User ID=your-user;Password=your-password;Database=your-database"
}
```

Also you need to add you JWT key, issuer and audience to the __appsettings.json__ file.
```json
"Jwt": {
    "Key": "your-key",
    "Issuer": "your-issuer",
    "Audience": "your-audience"
}
```

After you have setup you __appsettings.json__ file you should be able to run the solution
without any problem executing the following commands:
```bash
cd ./TodoListAPI
dotnet ef database update # To create de database an apply the migrations
dotnet run # To execute the solution
```

You can check all the endpoints and schemas used in each endpoint in the OpenAPI generated
documentation which is in the **/swagger** endpoint.

## Extras
### Unit Test
To successfully execute the unit tests, Docker must be installed, as they run on TestContainers. If you already have Docker installed, feel free to run the following commands:
```bash
cd ./TodoListAPITests
dotnet test
```

## Project URL
(TODO List API | Roadmap)[https://roadmap.sh/projects/todo-list-api]
