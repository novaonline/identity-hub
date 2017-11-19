FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app
# copy csproj and restore as distinct layers
COPY *.sln ./
COPY IdentityServ.Models/*.csproj ./IdentityServ.Models/
COPY IdentityServ.Rules/*.csproj ./IdentityServ.Rules/
COPY IdentityServer/*.csproj ./IdentityServer/
COPY IdentityServer.Tests/*.csproj ./IdentityServer.Tests/
COPY IdentityServerClient.CLI/*.csproj ./IdentityServerClient.CLI/
RUN dotnet restore *.sln

# copy everything else and build
COPY . ./
RUN dotnet publish IdentityServer/IdentityServer.csproj  -c Release -o out

# build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/IdentityServer/out .
ENTRYPOINT ["dotnet", "IdentityServer.dll"]
