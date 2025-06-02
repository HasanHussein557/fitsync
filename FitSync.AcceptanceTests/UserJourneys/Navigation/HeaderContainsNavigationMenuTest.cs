using System.Linq;
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
    public class HeaderContainsNavigationMenuTest : IClassFixture<TestWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestWebApplicationFactory _factory;

        public HeaderContainsNavigationMenuTest(TestWebApplicationFactory factory)
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

        [Fact]
        public async Task Test()
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