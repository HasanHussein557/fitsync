#!/bin/bash

# Stop and remove the existing MySQL container
echo "Stopping and removing the existing MySQL container..."
docker stop my-mysql-container
docker rm my-mysql-container

# Start the PostgreSQL container using Docker Compose
echo "Starting PostgreSQL container..."
docker-compose up -d

# Wait for PostgreSQL to start
echo "Waiting for PostgreSQL to initialize..."
sleep 10

# Initialize the database
echo "Creating database schema..."
docker exec -i fitsync-postgres psql -U fitsync -d fitsync < DAL/Scripts/CreateSchema.sql

# Insert sample data
echo "Inserting sample data..."
docker exec -i fitsync-postgres psql -U fitsync -d fitsync < DAL/Scripts/MigrateData.sql

echo "Migration complete! The application is now using PostgreSQL instead of MySQL."
echo "You can run the application with: dotnet run --project Presentation" 