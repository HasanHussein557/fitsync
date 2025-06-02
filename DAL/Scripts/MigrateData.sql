-- This is a reference for the migration process
-- This SQL will be executed against the PostgreSQL database
-- Data from MySQL will need to be exported and imported

-- Migrate Athletes data
-- INSERT INTO athletes (id, first_name, last_name, age, weight, height, sex, goal)
-- SELECT Id, FirstName, LastName, Age, Weight, Height, Sex, Goal FROM athletes_imported;

-- Migrate Users data
-- INSERT INTO users (id, username, email, password_hash, salt, athlete_id, created_date, last_login_date)
-- SELECT Id, Username, Email, PasswordHash, Salt, AthleteId, CreatedDate, LastLoginDate FROM users_imported;

-- Migrate UserRoles data
-- INSERT INTO user_roles (user_id, role)
-- SELECT UserId, Role FROM user_roles_imported;

-- Insert some common exercises
INSERT INTO exercises (name, category, primary_muscle_group, description) VALUES
('Bench Press', 'Strength', 'Chest', 'Compound exercise for chest, shoulders, and triceps'),
('Squat', 'Strength', 'Legs', 'Compound exercise for quadriceps, hamstrings, and glutes'),
('Deadlift', 'Strength', 'Back', 'Compound exercise for lower back, hamstrings, and glutes'),
('Pull-up', 'Strength', 'Back', 'Upper body exercise targeting the lats and biceps'),
('Push-up', 'Bodyweight', 'Chest', 'Bodyweight exercise for chest, shoulders, and triceps'),
('Plank', 'Core', 'Abdominals', 'Isometric core exercise that strengthens the abdominals and lower back'),
('Lunges', 'Strength', 'Legs', 'Single-leg exercise that targets quadriceps, hamstrings, and glutes'),
('Shoulder Press', 'Strength', 'Shoulders', 'Overhead press that targets the deltoids and triceps'),
('Bicep Curl', 'Isolation', 'Arms', 'Isolation exercise for the biceps'),
('Tricep Extension', 'Isolation', 'Arms', 'Isolation exercise for the triceps'),
('Lat Pulldown', 'Strength', 'Back', 'Upper body exercise that targets the latissimus dorsi'),
('Leg Press', 'Strength', 'Legs', 'Machine exercise that targets the quadriceps, hamstrings, and glutes'),
('Leg Curl', 'Isolation', 'Legs', 'Isolation exercise for the hamstrings'),
('Leg Extension', 'Isolation', 'Legs', 'Isolation exercise for the quadriceps'),
('Calf Raise', 'Isolation', 'Legs', 'Isolation exercise for the calves'),
('Dumbbell Row', 'Strength', 'Back', 'Unilateral exercise for the back and biceps'),
('Russian Twist', 'Core', 'Abdominals', 'Rotational exercise for the obliques'),
('Crunch', 'Core', 'Abdominals', 'Basic abdominal exercise'),
('Mountain Climber', 'Cardio', 'Full Body', 'Dynamic exercise that works the core, shoulders, and hips'),
('Burpee', 'Cardio', 'Full Body', 'Full body exercise that combines a squat, push-up, and jump');

-- Insert some common foods
INSERT INTO foods (name, calories, protein, carbs, fat, serving_size, serving_unit) VALUES
('Chicken Breast', 165, 31, 0, 3.6, 100, 'g'),
('Salmon', 206, 22, 0, 13, 100, 'g'),
('Brown Rice', 112, 2.6, 23.5, 0.9, 100, 'g'),
('Sweet Potato', 86, 1.6, 20.1, 0.1, 100, 'g'),
('Broccoli', 34, 2.8, 6.6, 0.4, 100, 'g'),
('Spinach', 23, 2.9, 3.6, 0.4, 100, 'g'),
('Egg', 78, 6.3, 0.6, 5.3, 1, 'large'),
('Greek Yogurt', 59, 10, 3.6, 0.4, 100, 'g'),
('Oatmeal', 68, 2.4, 12, 1.4, 100, 'g'),
('Banana', 89, 1.1, 22.8, 0.3, 100, 'g'),
('Apple', 52, 0.3, 13.8, 0.2, 100, 'g'),
('Avocado', 160, 2, 8.5, 14.7, 100, 'g'),
('Almonds', 576, 21.2, 21.7, 49.4, 100, 'g'),
('Olive Oil', 884, 0, 0, 100, 100, 'ml'),
('Whole Wheat Bread', 247, 13, 41, 3.4, 100, 'g'),
('Tuna', 116, 25.5, 0, 0.8, 100, 'g'),
('Quinoa', 120, 4.4, 21.3, 1.9, 100, 'g'),
('Lentils', 116, 9, 20, 0.4, 100, 'g'),
('Cottage Cheese', 98, 11.1, 3.4, 4.3, 100, 'g'),
('Peanut Butter', 588, 25, 20, 50, 100, 'g'); 