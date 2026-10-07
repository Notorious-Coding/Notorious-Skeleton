# Consultez https://aka.ms/customizecontainer pour savoir comment personnaliser votre conteneur de débogage et comment Visual Studio utilise ce Dockerfile pour générer vos images afin d’accélérer le débogage.

# Cet index est utilisé lors de l’exécution à partir de VS en mode rapide (par défaut pour la configuration de débogage)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Cette phase est utilisée pour générer le projet de service
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Directory.Packages.props", "."]
COPY ["1. Internal API/NotoriousSkeleton.API/NotoriousSkeleton.API.csproj", "1. Internal API/NotoriousSkeleton.API/"]

# Catalog module
COPY ["1. Internal API/NotoriousSkeleton.Catalog/NotoriousSkeleton.Catalog.csproj", "1. Internal API/NotoriousSkeleton.Catalog/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Application/NotoriousSkeleton.Catalog.Application.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Application/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Application.DependencyInjection.Microsoft/NotoriousSkeleton.Catalog.Application.DependencyInjection.Microsoft.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Application.DependencyInjection.Microsoft/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Authentication.Auth0/NotoriousSkeleton.Catalog.Authentication.Auth0.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Authentication.Auth0/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Authentication.Auth0.Extensions/NotoriousSkeleton.Catalog.Authentication.Auth0.Extensions.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Authentication.Auth0.Extensions/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Domain/NotoriousSkeleton.Catalog.Domain.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Domain/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Externals/NotoriousSkeleton.Catalog.Externals.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Externals/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Persistence.EntityFramework/NotoriousSkeleton.Catalog.Persistence.EntityFramework.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Persistence.EntityFramework/"]
COPY ["1. Internal API/NotoriousSkeleton.Catalog.Persistence.EntityFramework.Extensions/NotoriousSkeleton.Catalog.Persistence.EntityFramework.Extensions.csproj", "1. Internal API/NotoriousSkeleton.Catalog.Persistence.EntityFramework.Extensions/"]

# Ordering module
COPY ["1. Internal API/NotoriousSkeleton.Ordering/NotoriousSkeleton.Ordering.csproj", "1. Internal API/NotoriousSkeleton.Ordering/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Application/NotoriousSkeleton.Ordering.Application.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Application/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Application.DependencyInjection.Microsoft/NotoriousSkeleton.Ordering.Application.DependencyInjection.Microsoft.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Application.DependencyInjection.Microsoft/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Authentication.Auth0/NotoriousSkeleton.Ordering.Authentication.Auth0.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Authentication.Auth0/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Authentication.Auth0.Extensions/NotoriousSkeleton.Ordering.Authentication.Auth0.Extensions.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Authentication.Auth0.Extensions/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Client.Catalog/NotoriousSkeleton.Ordering.Client.Catalog.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Client.Catalog/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Client.Catalog.Extensions/NotoriousSkeleton.Ordering.Client.Catalog.Extensions.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Client.Catalog.Extensions/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Domain/NotoriousSkeleton.Ordering.Domain.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Domain/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Persistence.EntityFramework/NotoriousSkeleton.Ordering.Persistence.EntityFramework.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Persistence.EntityFramework/"]
COPY ["1. Internal API/NotoriousSkeleton.Ordering.Persistence.EntityFramework.Extensions/NotoriousSkeleton.Ordering.Persistence.EntityFramework.Extensions.csproj", "1. Internal API/NotoriousSkeleton.Ordering.Persistence.EntityFramework.Extensions/"]

# Shared kernel
COPY ["1. Internal API/NotoriousSkeleton.SharedKernel/NotoriousSkeleton.SharedKernel.csproj", "1. Internal API/NotoriousSkeleton.SharedKernel/"]

# Libraries
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Authentication/NotoriousSkeleton.Libraries.Authentication.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Authentication/"]
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Configuration/NotoriousSkeleton.Libraries.Configuration.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Configuration/"]
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Domain.Tools/NotoriousSkeleton.Libraries.Domain.Tools.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Domain.Tools/"]
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Enumeration/NotoriousSkeleton.Libraries.Enumeration.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Enumeration/"]
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Repositories/NotoriousSkeleton.Libraries.Repositories.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Repositories/"]
COPY ["3. Libraries/NotoriousSkeleton.Libraries.Result/NotoriousSkeleton.Libraries.Result.csproj", "3. Libraries/NotoriousSkeleton.Libraries.Result/"]
RUN dotnet restore "./1. Internal API/NotoriousSkeleton.API/NotoriousSkeleton.API.csproj"
COPY . .
WORKDIR "/src/1. Internal API/NotoriousSkeleton.API"
RUN dotnet build "./NotoriousSkeleton.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Cette étape permet de publier le projet de service à copier dans la phase finale
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotoriousSkeleton.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Cette phase est utilisée en production ou lors de l’exécution à partir de VS en mode normal (par défaut quand la configuration de débogage n’est pas utilisée)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotoriousSkeleton.API.dll"]
