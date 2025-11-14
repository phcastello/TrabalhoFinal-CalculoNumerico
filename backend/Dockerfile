FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY NumericalMethods.sln ./
COPY src/NumericalMethods.Core/NumericalMethods.Core.csproj src/NumericalMethods.Core/
COPY src/NumericalMethods.Api/NumericalMethods.Api.csproj src/NumericalMethods.Api/
RUN dotnet restore NumericalMethods.sln

COPY . .
RUN dotnet publish src/NumericalMethods.Api/NumericalMethods.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:5000
COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "NumericalMethods.Api.dll"]
