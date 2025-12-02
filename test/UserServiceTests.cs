using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Xunit;
using nera_cji.Interfaces.Services;
using nera_cji.Services;
using nera_cji.Models;

namespace Tests;

public class UserServiceTests {
    private class FakeEnv : IWebHostEnvironment {
        public string ApplicationName { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }

    [Fact]
    public async Task AddAndFindUser_RoundTrip_Works() {
        var env = new FakeEnv();
        IUserService store = new FileUserStore(env);

        var user = new User { FullName = "Test User", email = Guid.NewGuid() + "@example.com" };
        await store.AddAsync(user);

        var found = await store.FindByEmailAsync(user.email);
        Assert.NotNull(found);
        Assert.Equal(user.email.ToUpperInvariant(), found!.NormalizedEmail);
    }
}


