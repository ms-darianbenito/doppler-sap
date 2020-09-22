FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS restore
WORKDIR /src
COPY Doppler.Sap.sln ./
COPY Doppler.Sap/Doppler.Sap.csproj ./Doppler.Sap/Doppler.Sap.csproj
COPY Doppler.Sap.Test/Doppler.Sap.Test.csproj ./Doppler.Sap.Test/Doppler.Sap.Test.csproj
RUN dotnet restore

FROM restore AS build
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS test
RUN dotnet test

FROM build AS publish
RUN dotnet publish "Doppler.Sap/Doppler.Sap.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS final
WORKDIR /app
EXPOSE 80
COPY --from=publish /app/publish .
ARG version=unknown
RUN echo $version > /app/wwwroot/version.txt
ENTRYPOINT ["dotnet", "Doppler.Sap.dll"]
