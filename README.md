# FitSync - Database Refactoring

## Overview

This project has been refactored to use PostgreSQL instead of MySQL, with a normalized database schema that separates workouts, exercises, nutrition schemas, and meals into proper relational tables.

## Changes Made

1. **Database Migration**
   - Switched from MySQL to PostgreSQL
   - Created a normalized database schema with proper relationships
   - Added support for nutrition schema with meals and foods

2. **Schema Improvements**
   - Removed JSON storage in favor of proper relational tables
   - Created dedicated tables for exercises, workouts, nutrition schemas, meals, and foods
   - Added proper indexing for performance

3. **Code Refactoring**
   - Updated repositories to work with the new schema
   - Updated data models to include additional fields
   - Added support for transactions to maintain data integrity

## Setup Instructions

1. **Prerequisites**
   - Docker and Docker Compose
   - .NET 8.0 SDK

2. **Running the Application**
   ```
   # Start the PostgreSQL database
   docker-compose up -d
   
   # Initialize the database (create schema and insert sample data)
   ./init-postgres.sh
   
   # Run the application
   dotnet run --project Presentation
   ```

## Database Schema

### Athletes
- id (PK)
- first_name
- last_name
- age
- weight
- height
- sex
- goal

### Users
- id (PK)
- username (unique)
- email (unique)
- password_hash
- salt
- athlete_id (FK to athletes)
- created_date
- last_login_date

### User Roles
- user_id (PK, FK to users)
- role (PK)

### Exercises
- id (PK)
- name
- category
- primary_muscle_group
- description

### Workout Schemas
- id (PK)
- athlete_id (FK to athletes)
- name
- created_date
- workouts_per_week
- goal

### Workouts
- id (PK)
- workout_schema_id (FK to workout_schemas)
- name
- day_of_week
- workout_order
- notes

### Workout Exercises
- id (PK)
- workout_id (FK to workouts)
- exercise_id (FK to exercises)
- sets
- reps
- weight
- duration
- rest
- exercise_order
- notes

### Nutrition Schemas
- id (PK)
- athlete_id (FK to athletes)
- name
- created_date
- daily_calorie_target
- protein_target
- carb_target
- fat_target
- notes

### Meal Plans
- id (PK)
- nutrition_schema_id (FK to nutrition_schemas)
- name
- time
- meal_order

### Foods
- id (PK)
- name
- calories
- protein
- carbs
- fat
- serving_size
- serving_unit

### Meal Foods
- id (PK)
- meal_plan_id (FK to meal_plans)
- food_id (FK to foods)
- quantity
- notes 