using Microsoft.Extensions.Configuration;

namespace CoralLedger.E2E.Tests;

/// <summary>
/// Base test fixture for Playwright E2E tests.
/// Provides browser context and page management with configuration.
/// </summary>
public class PlaywrightFixture : PageTest
{
    protected string BaseUrl { get; private set; } = null!;
    protected IConfiguration Configuration { get; private set; } = null!;
    protected List<string> ConsoleErrors { get; } = new();

    [SetUp]
    public async Task BaseSetUp()
    {
        // Load configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(TestContext.CurrentContext.TestDirectory)
            .AddJsonFile("appsettings.e2e.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Get base URL from environment or config
        BaseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL")
            ?? Configuration["BaseUrl"]
            ?? "https://localhost:7232";

        // Track console errors
        Page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                ConsoleErrors.Add($"[{msg.Type}] {msg.Text}");
            }
        };

        // Configure default timeout
        var timeout = int.Parse(Configuration["Timeout"] ?? "30000");
        Page.SetDefaultTimeout(timeout);
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        // Take screenshot on failure
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotPath = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                "playwright-artifacts",
                $"{TestContext.CurrentContext.Test.Name}-failure.png");

            Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
            await Page.ScreenshotAsync(new() { Path = screenshotPath, FullPage = true });
            TestContext.AddTestAttachment(screenshotPath, "Failure Screenshot");
        }

        // Clear console errors for next test
        ConsoleErrors.Clear();
    }

    protected async Task WaitForBlazorAsync()
    {
        // Wait for Blazor to initialize
        await Page.WaitForFunctionAsync("window.Blazor !== undefined");
        // Give a brief moment for initial render
        await Task.Delay(500);
    }

    protected async Task NavigateToAsync(string path)
    {
        await Page.GotoAsync($"{BaseUrl}{path}", new()
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
        await WaitForBlazorAsync();
    }

    protected void AssertNoConsoleErrors()
    {
        ConsoleErrors.Should().BeEmpty("Page should not have console errors");
    }
}
