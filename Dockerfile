# Etapa 1 - Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copia os arquivos e restaura dependências
COPY *.sln .
COPY FinancialManagerAPI/*.csproj ./FinancialManagerAPI/
RUN dotnet restore

# Copia o restante do código
COPY FinancialManagerAPI/. ./FinancialManagerAPI/
WORKDIR /app/FinancialManagerAPI
RUN dotnet publish -c Release -o out

# Etapa 2 - Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/FinancialManagerAPI/out ./

# Define a porta que o Render vai expor (Render espera que a aplicação escute em 0.0.0.0:10000)
ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV DOTNET_EnableDiagnostics=0

# Executa o app
ENTRYPOINT ["dotnet", "FinancialManagerAPI.dll"]