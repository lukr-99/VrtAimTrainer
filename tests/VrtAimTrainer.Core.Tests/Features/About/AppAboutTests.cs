using VrtAimTrainer.Core.Features.About;

namespace VrtAimTrainer.Core.Tests.Features.About;

public sealed class AppAboutTests
{
    [Fact]
    public void Links_AreSecureAbsoluteUrlsWithUniqueIds()
    {
        Assert.Equal(AppAbout.Links.Count, AppAbout.Links.Select(link => link.Id).Distinct().Count());
        foreach (var link in AppAbout.Links)
        {
            Assert.True(Uri.TryCreate(link.Url, UriKind.Absolute, out var uri));
            Assert.Equal(Uri.UriSchemeHttps, uri!.Scheme);
        }
    }

    [Fact]
    public void Links_CoverInstagramGitHubAndIssues()
    {
        var urls = AppAbout.Links.Select(link => link.Url).ToArray();
        Assert.Contains("https://www.instagram.com/lukas9krejci/", urls);
        Assert.Contains("https://github.com/lukr-99", urls);
        Assert.Contains("https://github.com/lukr-99/VrtAimTrainer/issues", urls);
    }

    [Fact]
    public void Text_HasNoEmDashes()
    {
        var all = new[] { AppAbout.Tagline, AppAbout.Summary, AppAbout.FeedbackInvitation }
            .Concat(AppAbout.Credits.SelectMany(credit => new[] { credit.Name, credit.Role, credit.License }));
        Assert.All(all, text => Assert.DoesNotContain('\u2014', text));
    }
}
