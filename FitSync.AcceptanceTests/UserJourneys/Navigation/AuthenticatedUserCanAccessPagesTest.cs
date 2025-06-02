using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FitSync.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FitSync.AcceptanceTests.UserJourneys.Navigation
{
    public class AuthenticatedUserCanAccessPagesTest : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public AuthenticatedUserCanAccessPagesTest(TestWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddAuthentication("Test")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
                });
            }).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = true
            });
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Index")]
        [InlineData("/Account/Profile")]
        [InlineData("/GenerateNutrition")]
        [InlineData("/WorkoutSchema")]
        [InlineData("/WorkoutSchemas")]
        [InlineData("/Privacy")]
        public async Task Test(string url)
        {
            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
} 