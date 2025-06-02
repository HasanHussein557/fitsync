using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FitSync.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FitSync.AcceptanceTests.UserJourneys.Navigation
{
    public class AnonymousUserIsRedirectedToLoginTest : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public AnonymousUserIsRedirectedToLoginTest(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/Account/Profile")]
        [InlineData("/WorkoutSchema")]
        [InlineData("/WorkoutSchemas")]
        public async Task Test(string url)
        {
            // Arrange
            var anonymousClient = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await anonymousClient.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Account/Login", response.Headers.Location.ToString());
        }
    }
} 