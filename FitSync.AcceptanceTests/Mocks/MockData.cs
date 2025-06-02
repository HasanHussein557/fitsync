using System;
using System.Collections.Generic;
using Core.Domain.Entities;
using static Presentation.Pages.WorkoutSchemaModel;
using static Presentation.Pages.GenerateNutritionModel;

namespace FitSync.AcceptanceTests.Mocks
{
    public static class MockData
    {
        public static User TestUser => new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            CreatedDate = DateTime.Now.AddMonths(-3),
            AthleteId = 1
        };
        
        public static Athlete TestAthlete => new Athlete
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Age = 30,
            Height = 180,
            Weight = 75,
            Sex = "Male",
            Goal = "Muscle gain"
        };
        
        public static string TestWorkoutPlan => @"{
            ""warmup"": {
                ""description"": ""5-10 minutes of light cardio"",
                ""exercises"": [""Light jogging"", ""Dynamic stretching""],
                ""duration_minutes"": 10
            },
            ""sessions_per_week"": 3,
            ""workout_sessions"": [
                {
                    ""name"": ""Upper Body Workout"",
                    ""target_muscles"": [""Chest"", ""Back"", ""Shoulders"", ""Arms""],
                    ""exercises"": [
                        {
                            ""name"": ""Bench Press"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""90s""
                        },
                        {
                            ""name"": ""Lat Pulldown"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""90s""
                        }
                    ],
                    ""duration_minutes"": 45
                },
                {
                    ""name"": ""Lower Body Workout"",
                    ""target_muscles"": [""Legs"", ""Glutes"", ""Core""],
                    ""exercises"": [
                        {
                            ""name"": ""Squats"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""90s""
                        },
                        {
                            ""name"": ""Deadlifts"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""120s""
                        }
                    ],
                    ""duration_minutes"": 45
                },
                {
                    ""name"": ""Full Body Workout"",
                    ""target_muscles"": [""Full Body""],
                    ""exercises"": [
                        {
                            ""name"": ""Push-ups"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""60s""
                        },
                        {
                            ""name"": ""Pull-ups"",
                            ""sets"": 3,
                            ""reps"": ""8-12"",
                            ""rest"": ""60s""
                        }
                    ],
                    ""duration_minutes"": 50
                }
            ],
            ""cooldown"": {
                ""description"": ""Static stretching for all major muscle groups"",
                ""exercises"": [""Hamstring stretch"", ""Quad stretch"", ""Chest stretch""],
                ""duration_minutes"": 5
            }
        }";
        
        public static string TestNutritionPlan => @"{
            ""daily_calories_range"": {
                ""min"": 2200,
                ""max"": 2800
            },
            ""macronutrients_range"": {
                ""protein"": {
                    ""min"": 160,
                    ""max"": 220
                },
                ""carbs"": {
                    ""min"": 220,
                    ""max"": 330
                },
                ""fat"": {
                    ""min"": 60,
                    ""max"": 90
                }
            },
            ""meal_plan"": {
                ""breakfast"": [
                    {
                        ""description"": ""Protein Oatmeal with Fruit"",
                        ""ingredients"": [
                            {
                                ""ingredient"": ""Oatmeal"",
                                ""quantity"": ""1 cup"",
                                ""calories"": 300
                            },
                            {
                                ""ingredient"": ""Whey Protein"",
                                ""quantity"": ""1 scoop"",
                                ""calories"": 120
                            },
                            {
                                ""ingredient"": ""Banana"",
                                ""quantity"": ""1 medium"",
                                ""calories"": 105
                            }
                        ],
                        ""total_calories"": 525,
                        ""recipe"": ""Mix oatmeal with water, cook, then stir in protein powder and top with sliced banana.""
                    }
                ],
                ""lunch"": [
                    {
                        ""description"": ""Chicken and Rice Bowl"",
                        ""ingredients"": [
                            {
                                ""ingredient"": ""Chicken Breast"",
                                ""quantity"": ""6 oz"",
                                ""calories"": 280
                            },
                            {
                                ""ingredient"": ""Brown Rice"",
                                ""quantity"": ""1 cup"",
                                ""calories"": 215
                            },
                            {
                                ""ingredient"": ""Broccoli"",
                                ""quantity"": ""1 cup"",
                                ""calories"": 55
                            }
                        ],
                        ""total_calories"": 550,
                        ""recipe"": ""Cook rice according to package. Grill chicken seasoned with herbs. Steam broccoli. Combine in a bowl.""
                    }
                ],
                ""dinner"": [
                    {
                        ""description"": ""Salmon with Sweet Potato and Asparagus"",
                        ""ingredients"": [
                            {
                                ""ingredient"": ""Salmon"",
                                ""quantity"": ""6 oz"",
                                ""calories"": 350
                            },
                            {
                                ""ingredient"": ""Sweet Potato"",
                                ""quantity"": ""1 medium"",
                                ""calories"": 180
                            },
                            {
                                ""ingredient"": ""Asparagus"",
                                ""quantity"": ""1 cup"",
                                ""calories"": 40
                            }
                        ],
                        ""total_calories"": 570,
                        ""recipe"": ""Bake salmon with lemon and herbs. Roast sweet potato and asparagus with olive oil.""
                    }
                ],
                ""snacks"": [
                    {
                        ""description"": ""Greek Yogurt with Berries and Nuts"",
                        ""ingredients"": [
                            {
                                ""ingredient"": ""Greek Yogurt"",
                                ""quantity"": ""1 cup"",
                                ""calories"": 150
                            },
                            {
                                ""ingredient"": ""Mixed Berries"",
                                ""quantity"": ""1/2 cup"",
                                ""calories"": 40
                            },
                            {
                                ""ingredient"": ""Almonds"",
                                ""quantity"": ""1 oz"",
                                ""calories"": 160
                            }
                        ],
                        ""total_calories"": 350,
                        ""recipe"": ""Mix plain Greek yogurt with berries and top with almonds.""
                    }
                ]
            }
        }";
    }
} 