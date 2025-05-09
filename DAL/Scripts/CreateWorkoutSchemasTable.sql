-- Create WorkoutSchemas table
CREATE TABLE IF NOT EXISTS WorkoutSchemas (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    AthleteId INT NOT NULL,
    Name VARCHAR(100),
    CreatedDate DATETIME NOT NULL,
    WorkoutsPerWeek INT NOT NULL,
    Goal VARCHAR(50),
    WorkoutsJson LONGTEXT,
    FOREIGN KEY (AthleteId) REFERENCES Athletes(Id) ON DELETE CASCADE
);

-- Create index for faster lookups by AthleteId
CREATE INDEX IX_WorkoutSchemas_AthleteId ON WorkoutSchemas(AthleteId); 