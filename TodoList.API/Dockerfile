FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["TodoList.API/TodoList.API.csproj", "TodoList.API/"]
RUN dotnet restore "TodoList.API/TodoList.API.csproj"
COPY . .
WORKDIR "/src/TodoList.API"
RUN dotnet build "TodoList.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TodoList.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TodoList.API.dll"]