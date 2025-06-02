using Core.Domain.Entities;
using Core.Interfaces;
using FitSync.AcceptanceTests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSync.AcceptanceTests.Helpers
{
    public class TestWebApplicationFactory : WebApplicationFactory<Presentation.Pages.ErrorModel>
    {
        // Mocks
        public Mock<IAthleteRepository> AthleteRepositoryMock { get; } = new Mock<IAthleteRepository>();
        public Mock<IUserRepository> UserRepositoryMock { get; } = new Mock<IUserRepository>();
        public Mock<IWorkoutSchemaGenerator> WorkoutSchemaGeneratorMock { get; } = new Mock<IWorkoutSchemaGenerator>();
        public Mock<INutritionSchemaGenerator> NutritionSchemaGeneratorMock { get; } = new Mock<INutritionSchemaGenerator>();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Register mocks
                services.AddScoped<IAthleteRepository>(_ => AthleteRepositoryMock.Object);
                services.AddScoped<IUserRepository>(_ => UserRepositoryMock.Object);
                services.AddScoped<IWorkoutSchemaGenerator>(_ => WorkoutSchemaGeneratorMock.Object);
                services.AddScoped<INutritionSchemaGenerator>(_ => NutritionSchemaGeneratorMock.Object);
                
                // Set up mock data
                SetupMockData();
            });
        }
        
        private void SetupMockData()
        {
            // Mock user data
            UserRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync("testuser"))
                .ReturnsAsync(MockData.TestUser);
            
            UserRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(MockData.TestUser);
            
            // Mock athlete data
            AthleteRepositoryMock.Setup(repo => repo.GetAthleteByIdAsync(1))
                .ReturnsAsync(MockData.TestAthlete);
                
            AthleteRepositoryMock.Setup(repo => repo.GetAllAthletesAsync())
                .ReturnsAsync(new List<Athlete> { MockData.TestAthlete });
                
            // Mock workout schema generation
            WorkoutSchemaGeneratorMock.Setup(generator => 
                generator.GenerateWorkoutSchemaAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(MockData.TestWorkoutPlan);
                
            // Mock nutrition plan generation
            NutritionSchemaGeneratorMock.Setup(generator => 
                generator.GenerateNutritionSchemaAsync(
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), 
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), 
                    It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(MockData.TestNutritionPlan);
        }
    }
} 