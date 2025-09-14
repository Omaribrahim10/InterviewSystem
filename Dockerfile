# Use official ASP.NET runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Use official SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything and restore/build
COPY . .
RUN dotnet restore "./InterviewsApplication/InterviewsApplication.csproj"
RUN dotnet publish "./InterviewsApplication/InterviewsApplication.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InterviewsApplication.dll"]
