using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FitSync.AcceptanceTests.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Linq;

namespace FitSync.AcceptanceTests.UserJourneys
{
    public class NavigationTests : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public NavigationTests(TestWebApplicationFactory factory)
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
        public async Task Authenticated_User_Can_Access_Pages(string url)
        {
            // Act
            var response = await _client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Account/Login")]
        [InlineData("/Account/Register")]
        public async Task Anonymous_User_Can_Access_Auth_Pages(string url)
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

        [Theory]
        [InlineData("/Account/Profile")]
        [InlineData("/WorkoutSchema")]
        [InlineData("/WorkoutSchemas")]
        public async Task Anonymous_User_Is_Redirected_To_Login(string url)
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

        [Fact]
        public async Task Header_Contains_Navigation_Menu()
        {
            // Act
            var response = await _client.GetAsync("/");
            var document = await HtmlHelpers.GetDocumentAsync(response);

            // Assert
            var menuItems = document.QuerySelectorAll(".menu-item");
            Assert.NotEmpty(menuItems);
            
            // Check for menu items - revise these assertions based on your actual menu item labels
            var menuTexts = menuItems.Select(item => item.TextContent).ToList();
            Assert.Contains(menuTexts, text => text.Contains("Home"));
        }
    }
} 