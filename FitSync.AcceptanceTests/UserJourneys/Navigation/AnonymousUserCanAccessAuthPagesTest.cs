using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FitSync.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FitSync.AcceptanceTests.UserJourneys.Navigation
{
    public class AnonymousUserCanAccessAuthPagesTest : IClassFixture<TestWebApplicationFactory>
    {
        private readonly TestWebApplicationFactory _factory;

        public AnonymousUserCanAccessAuthPagesTest(TestWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/Account/Login/")]
        [InlineData("/Account/Register")]
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
} 