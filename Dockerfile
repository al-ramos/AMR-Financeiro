FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os .csproj de cada camada e restaura dependências
COPY src/AMR.Financeiro.Shared/AMR.Financeiro.Shared.csproj           src/AMR.Financeiro.Shared/
COPY src/AMR.Financeiro.Domain/AMR.Financeiro.Domain.csproj           src/AMR.Financeiro.Domain/
COPY src/AMR.Financeiro.Application/AMR.Financeiro.Application.csproj src/AMR.Financeiro.Application/
COPY src/AMR.Financeiro.Infrastructure/AMR.Financeiro.Infrastructure.csproj src/AMR.Financeiro.Infrastructure/
COPY src/AMR.Financeiro.API/AMR.Financeiro.API.csproj                 src/AMR.Financeiro.API/

RUN dotnet restore src/AMR.Financeiro.API/AMR.Financeiro.API.csproj

# Copia o restante e publica
COPY src/ src/
RUN dotnet publish src/AMR.Financeiro.API/AMR.Financeiro.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AMR.Financeiro.API.dll"]
