using CryptoPriceAIAssistance.Cache;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;

namespace Tests;

[TestFixture]
public class CacheServiceTests
{
    private string testCacheFilePath;
    private CacheService _cacheService;

    [SetUp]
    public void SetUp()
    {
        // Create a unique temporary file for testing
        testCacheFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
        _cacheService = new CacheService(testCacheFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary file after tests
        if(File.Exists(testCacheFilePath))
        {
            File.Delete(testCacheFilePath);
        }
    }

    [Test]
    public void Get_ShouldReturnNull_WhenKeyDoesNotExist()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=NONEXISTENT";

        // Act
        var result = _cacheService.Get(key);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void Set_And_Get_ShouldStoreAndRetrieveValue()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=BTC";
        string value = "{\"price\": 50000}";

        // Act
        _cacheService.Set(key, value);
        var result = _cacheService.Get(key);

        // Assert
        result.Should().Be(value);
    }

    [Test]
    public void Set_ShouldOverwriteExistingValue()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=ETH";
        string initialValue = "{\"price\": 4000}";
        string updatedValue = "{\"price\": 4500}";

        // Act
        _cacheService.Set(key, initialValue);
        _cacheService.Set(key, updatedValue);
        var result = _cacheService.Get(key);

        // Assert
        result.Should().Be(updatedValue);
    }

    [Test]
    public void Cache_ShouldPersistAcrossInstances()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=XRP";
        string value = "{\"price\": 1}";

        // Act
        _cacheService.Set(key, value);

        // Create a new instance pointing to the same cache file
        var newCacheService = new CacheService(testCacheFilePath);
        var result = newCacheService.Get(key);

        // Assert
        result.Should().Be(value);
    }

    [Test]
    public void Cache_ShouldReset_WhenDateHasChanged()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=LTC";
        string value = "{\"price\": 200}";
        _cacheService.Set(key, value);

        // Act
        var result = _cacheService.Get(key);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public void CacheEntry_ShouldHaveCorrectProperties()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=ADA";
        string value = "{\"price\": 2}";

        // Act
        _cacheService.Set(key, value);
        var retrievedValue = _cacheService.Get(key);

        // Assert
        retrievedValue.Should().Be(value);

        // Additionally, verify the cache file content
        string json = File.ReadAllText(testCacheFilePath);
        var cacheData = JsonSerializer.Deserialize<CacheData>(json);
        cacheData.Should().NotBeNull();
        cacheData.Entries.Should().ContainKey(key);

        var entry = cacheData.Entries[key];
        entry.Url.Should().Be(key);
        entry.Response.Should().Be(value);
        entry.CachedAt.Should().BeCloseTo(DateTime.Now, precision: TimeSpan.FromSeconds(5));
    }

    [Test]
    public void ResetIfNeeded_ShouldClearCache_OnNewDay()
    {
        // Arrange
        string key = "https://api.crypto.com/price?symbol=DOGE";
        string value = "{\"price\": 0.25}";
        _cacheService.Set(key, value);

        // Simulate date change by setting cache date to yesterday
        var cacheData = new CacheData
        {
            CacheDate = DateTime.Today.AddDays(-1),
            Entries = new ConcurrentDictionary<string, CacheEntry>()
        };
        string json = JsonSerializer.Serialize(cacheData);
        File.WriteAllText(testCacheFilePath, json);

        // Act
        var result = _cacheService.Get(key);

        // Assert
        result.Should().BeNull();

        // Verify that the cache date has been updated to today
        var updatedJson = File.ReadAllText(testCacheFilePath);
        var updatedCacheData = JsonSerializer.Deserialize<CacheData>(updatedJson);
        updatedCacheData.CacheDate.Date.Should().Be(DateTime.Today);
        updatedCacheData.Entries.Should().BeEmpty();
    }
}

