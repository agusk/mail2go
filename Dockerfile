# ─────────────────────────────────────────────
# Stage 1: Build
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /repo

# Copy solution and project files first for layer caching
COPY Mail2Go.slnx ./
COPY src/Mail2Go.Domain/Mail2Go.Domain.csproj             src/Mail2Go.Domain/
COPY src/Mail2Go.Application/Mail2Go.Application.csproj   src/Mail2Go.Application/
COPY src/Mail2Go.Infrastructure/Mail2Go.Infrastructure.csproj src/Mail2Go.Infrastructure/
COPY src/Mail2Go.Web/Mail2Go.Web.csproj                   src/Mail2Go.Web/

# Restore packages
RUN dotnet restore src/Mail2Go.Web/Mail2Go.Web.csproj

# Copy the rest of the source
COPY src/ src/

# Publish
RUN dotnet publish src/Mail2Go.Web/Mail2Go.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ─────────────────────────────────────────────
# Stage 2: Runtime
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app

# Install ICU libraries (required for .NET globalization on Alpine)
RUN apk add --no-cache icu-libs

# Non-root user for security
RUN addgroup -S mail2go && adduser -S mail2go -G mail2go

# Create data directory and give ownership
RUN mkdir -p /app/data && chown mail2go:mail2go /app/data

# Copy published output
COPY --from=build /app/publish .

# Give the app user ownership of the app directory
RUN chown -R mail2go:mail2go /app

USER mail2go

# Web UI
EXPOSE 5050
# SMTP server
EXPOSE 2525

ENV ASPNETCORE_URLS=http://+:5050
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

VOLUME ["/app/data"]

ENTRYPOINT ["dotnet", "Mail2Go.Web.dll"]
