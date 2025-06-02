#!/bin/bash

# Start the Docker Compose services
docker-compose up -d

# Wait a moment for the database to initialize
echo "Waiting for PostgreSQL to start up..."
sleep 10

# Create the database schema
echo "Creating database schema..."
docker exec -i fitsync-postgres psql -U fitsync -d fitsync < DAL/Scripts/CreateSchema.sql

# Insert sample data
echo "Inserting sample data..."
docker exec -i fitsync-postgres psql -U fitsync -d fitsync < DAL/Scripts/MigrateData.sql

echo "Database initialization complete!" 