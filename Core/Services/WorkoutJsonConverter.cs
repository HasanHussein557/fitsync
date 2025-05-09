using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core.Domain.Entities;

namespace Core.Services
{
    public class WorkoutJsonConverter : JsonConverter<List<Workout>>
    {
        public override List<Workout> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected start of array");
            }

            var workouts = new List<Workout>();
            var exerciseConverter = new ExerciseJsonConverter();
            
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return workouts;
                }

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected start of object");
                }

                var workout = new Workout
                {
                    Exercises = new List<Exercise>()
                };

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        throw new JsonException("Expected property name");
                    }

                    string propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName.ToLower())
                    {
                        case "id":
                            workout.Id = reader.GetInt32();
                            break;
                        case "name":
                            workout.Name = reader.GetString();
                            break;
                        case "dayofweek":
                            workout.DayOfWeek = reader.GetInt32();
                            break;
                        case "exercises":
                            if (reader.TokenType == JsonTokenType.StartArray)
                            {
                                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                                {
                                    if (reader.TokenType == JsonTokenType.StartObject)
                                    {
                                        var exercise = JsonSerializer.Deserialize<Exercise>(ref reader, options);
                                        if (exercise != null)
                                        {
                                            workout.Exercises.Add(exercise);
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                workouts.Add(workout);
            }

            throw new JsonException("Expected end of array");
        }

        public override void Write(Utf8JsonWriter writer, List<Workout> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var workout in value)
            {
                writer.WriteStartObject();
                
                writer.WriteNumber("Id", workout.Id);
                writer.WriteString("Name", workout.Name);
                writer.WriteNumber("DayOfWeek", workout.DayOfWeek);

                writer.WritePropertyName("Exercises");
                writer.WriteStartArray();
                
                foreach (var exercise in workout.Exercises)
                {
                    writer.WriteStartObject();
                    
                    writer.WriteNumber("Id", exercise.Id);
                    writer.WriteString("Name", exercise.Name);
                    writer.WriteNumber("Sets", exercise.Sets);
                    writer.WriteString("Reps", exercise.Reps);
                    writer.WriteString("Rest", exercise.Rest);
                    
                    writer.WriteEndObject();
                }
                
                writer.WriteEndArray();
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
    
    public class ExerciseJsonConverter : JsonConverter<Exercise>
    {
        public override Exercise Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected start of object");
            }

            var exercise = new Exercise();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return exercise;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected property name");
                }

                string propertyName = reader.GetString();
                reader.Read();

                switch (propertyName.ToLower())
                {
                    case "id":
                        exercise.Id = reader.GetInt32();
                        break;
                    case "name":
                        exercise.Name = reader.GetString();
                        break;
                    case "sets":
                        exercise.Sets = reader.GetInt32();
                        break;
                    case "reps":
                        exercise.Reps = reader.GetString();
                        break;
                    case "rest":
                        exercise.Rest = reader.GetString();
                        break;
                }
            }

            throw new JsonException("Expected end of object");
        }

        public override void Write(Utf8JsonWriter writer, Exercise value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            
            writer.WriteNumber("Id", value.Id);
            writer.WriteString("Name", value.Name);
            writer.WriteNumber("Sets", value.Sets);
            writer.WriteString("Reps", value.Reps);
            writer.WriteString("Rest", value.Rest);
            
            writer.WriteEndObject();
        }
    }
} 