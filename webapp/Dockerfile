### BUILD ###
FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
RUN apk add --update npm
WORKDIR /src
COPY ./webapp.csproj ./webapp/
RUN dotnet restore "webapp/webapp.csproj"
COPY . ./webapp/
WORKDIR /src/webapp
RUN dotnet publish "webapp.csproj" -c Release -o /app/publish

### FINAL ###
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS http://*:80
EXPOSE 80
ENTRYPOINT ["dotnet", "webapp.dll"]
