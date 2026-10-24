FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY UserManagementAPI/UserManagementAPI.csproj UserManagementAPI/
RUN dotnet restore UserManagementAPI/UserManagementAPI.csproj
COPY UserManagementAPI/ UserManagementAPI/
RUN dotnet publish UserManagementAPI/UserManagementAPI.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "UserManagementAPI.dll"]
