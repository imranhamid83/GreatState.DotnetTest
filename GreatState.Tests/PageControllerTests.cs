using System;
using System.Collections.Generic;
using System.Linq;
using GreatState.DotnetTest.API;
using GreatState.DotnetTest.API.Controllers;
using GreatState.DotnetTest.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace GreatState.Tests
{
    public class PagesControllerTests
    {
        private CMSPages GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<CMSPages>()
                .UseInMemoryDatabase(dbName)
                .Options;
            var db = new CMSPages(options);
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            return db;
        }

        private PagesController GetController(string dbName)
        {
            var db = GetDbContext(dbName);
            var cache = new MemoryCache(new MemoryCacheOptions());
            return new PagesController(db, cache);
        }

        [Fact]
        public void GetPages_InvalidKey_ReturnsBadRequest()
        {
            var controller = GetController("test1");

            var result = controller.GetPages("admin", "wrong-key") as BadRequestObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPages_ValidRoleKey_ReturnsPages()
        {
            var controller = GetController("test2");

            var result = controller.GetPages("admin", "1234-admin") as OkObjectResult;

            Assert.NotNull(result);
            var data = result.Value?.GetType().GetProperty("data")?.GetValue(result.Value) as IEnumerable<Page>;
            Assert.NotNull(data);
            Assert.All(data, p => Assert.Equal("admin", p.RoleRequired));
        }

        [Fact]
        public void PostPage_AsAdmin_CreatesPage()
        {
            var controller = GetController("test3");

            var page = new Page { Title = "Test", Body = "Body", RoleRequired = "staff" };
            var result = controller.CreatePage("admin", "1234-admin", page) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Contains("successfully", result.Value?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PostPage_AsAnonymous_ReturnsUnauthorized()
        {
            var controller = GetController("test4");

            var page = new Page { Title = "Test", Body = "Body", RoleRequired = "staff" };
            var result = controller.CreatePage("anonymous", "1234-anon", page) as UnauthorizedObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void PutPage_NotFound_ReturnsNotFound()
        {
            var controller = GetController("test5");

            var page = new Page { Title = "Does not exist", Body = "Body", RoleRequired = "staff" };
            var result = controller.UpdatePage(99, "admin", "1234-admin", page) as NotFoundObjectResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void GetPages_UsesCache_OnSecondCall()
        {
            var controller = GetController("test6");

            // First call should populate cache
            var first = controller.GetPages("admin", "1234-admin", useCache: true) as OkObjectResult;
            var cachedFlag1 = first?.Value?.GetType().GetProperty("cached")?.GetValue(first.Value);
            Assert.Equal(false, cachedFlag1);

            // Second call should hit cache
            var second = controller.GetPages("admin", "1234-admin", useCache: true) as OkObjectResult;
            var cachedFlag2 = second?.Value?.GetType().GetProperty("cached")?.GetValue(second.Value);
            Assert.Equal(true, cachedFlag2);
        }

        [Fact]
        public void GetPages_DbDown_Returns500()
        {
            var options = new DbContextOptionsBuilder<CMSPages>()
                .UseInMemoryDatabase("GreatState")
                .Options;
            var db = new CMSPages(options);
            db.Dispose(); // simulate DB down

            var cache = new MemoryCache(new MemoryCacheOptions());
            var controller = new PagesController(db, cache);

            // Act
            var result = controller.GetPages("sitecore/admin", "1234-admin", useCache: true) as ObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(500, result.StatusCode);
        }
    }
}
