namespace nera_cji.Services;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using nera_cji.Interfaces.Services;
using nera_cji.Models;

public class FileUserStore : IUserService {
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly JsonSerializerOptions _serializerOptions = new() {
        WriteIndented = true
    };

    public FileUserStore(IWebHostEnvironment environment) {
        var dataDir = Path.Combine(environment.ContentRootPath, "Data");
        Directory.CreateDirectory(dataDir);
        _filePath = Path.Combine(dataDir, "users.json");
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(email)) {
            return null;
        }

        var normalizedEmail = email.ToUpperInvariant();
        var users = await LoadUsersAsync(cancellationToken);
        return users.FirstOrDefault(user => user.NormalizedEmail == normalizedEmail);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) {
        var user = await FindByEmailAsync(email, cancellationToken);
        return user != null;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) {
        await _lock.WaitAsync(cancellationToken);
        try {
            var users = await LoadUsersInternalAsync(cancellationToken);
            users.RemoveAll(existing => existing.NormalizedEmail == user.NormalizedEmail);
            users.Add(user);
            await SaveUsersInternalAsync(users, cancellationToken);
        }
        finally {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default) {
        var users = await LoadUsersAsync(cancellationToken);
        return users.AsReadOnly();
    }

    private async Task<List<User>> LoadUsersAsync(CancellationToken cancellationToken) {
        await _lock.WaitAsync(cancellationToken);
        try {
            return await LoadUsersInternalAsync(cancellationToken);
        }
        finally {
            _lock.Release();
        }
    }

    private async Task<List<User>> LoadUsersInternalAsync(CancellationToken cancellationToken) {
        if (!File.Exists(_filePath)) {
            return new List<User>();
        }

        await using var stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return await JsonSerializer.DeserializeAsync<List<User>>(stream, _serializerOptions, cancellationToken)
               ?? new List<User>();
    }

    private async Task SaveUsersInternalAsync(List<User> users, CancellationToken cancellationToken) {
        await using var stream = File.Open(_filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, users, _serializerOptions, cancellationToken);
    }
}


