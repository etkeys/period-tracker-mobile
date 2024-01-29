using System.Net;
using Moq;
using Moq.Protected;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, MemberData(nameof(GetLatestVersionTestsData))]
    public async Task GetLatestVersionTests(TestCase test){

        SetupHttpClientFactoryMock(test.Setups["httpclientfactory"]!);

        using var actor = new UpdateService(_httpClientFactoryMock.Object, _dbContextProviderMock.Object);
        var actVersion = await actor.GetLatestVersion();

        var expVersion = (Version?)test.Expected["version"];

        Assert.Equal(expVersion, actVersion);
    }

    private void SetupHttpClientFactoryMock(object httpClientFactorySetup){
        var setupData = (httpClientFactorySetup as Dictionary<string, object>)!;

        // ref: https://stackoverflow.com/a/44028625
        var mh = new Mock<HttpMessageHandler>();
        mh.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage{
                StatusCode = (HttpStatusCode)setupData["status code"],
                Content = (StringContent)setupData["content"]
            });

        _httpClientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mh.Object));
    }

    public static IEnumerable<object[]> GetLatestVersionTestsData => BundleTestCases(
        new TestCase("200 with correct data")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.OK},
                    {"content", new StringContent(_json_200_with_correct_data)}
                })
            .WithExpected("version", new Version("0.1.0"))

        ,new TestCase("200 but no json")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.OK},
                    {"content", new StringContent(string.Empty)}
                })
            .WithExpected("version", null)

        ,new TestCase("200 but tag_name is not present")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.OK},
                    {"content", new StringContent(_json_200_without_tag_name)}
                })
            .WithExpected("version", null)

        ,new TestCase("200 but tag_name is a version string")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.OK},
                    {"content", new StringContent(_json_200_tag_name_is_not_version)}
                })
            .WithExpected("version", null)

        ,new TestCase("400")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.BadRequest},
                    {"content", new StringContent(string.Empty)}
                })
            .WithExpected("version", null)

        ,new TestCase("404")
            .WithSetup(
                "httpclientfactory",
                new Dictionary<string, object>{
                    {"status code", HttpStatusCode.NotFound},
                    {"content", new StringContent(string.Empty)}
                })
            .WithExpected("version", null)

        // timeout

        );

    private const string _json_200_tag_name_is_not_version = @"
    {
        ""url"": ""https://api.anyurl.com"",
        ""html_url"": ""https://anyurl.com/releases/tag/0.1.0"",
        ""tag_name"": ""v0.1.0"",
        ""body"":""Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas eu tellus dui. Quisque vestibulum tortor elit, nec ultricies odio viverra quis. In scelerisque, leo at tincidunt interdum, nibh neque tempus felis, eu maximus tellus ipsum quis ligula. Mauris posuere lectus ut ullamcorper vestibulum. Nullam quis lectus ac nibh semper ornare in ullamcorper leo. Sed vitae lectus quis magna euismod consectetur. Ut ac pharetra massa.""
    }
    ";

    private const string _json_200_with_correct_data = @"
    {
        ""url"": ""https://api.anyurl.com"",
        ""html_url"": ""https://anyurl.com/releases/tag/0.1.0"",
        ""tag_name"": ""0.1.0"",
        ""body"":""Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas eu tellus dui. Quisque vestibulum tortor elit, nec ultricies odio viverra quis. In scelerisque, leo at tincidunt interdum, nibh neque tempus felis, eu maximus tellus ipsum quis ligula. Mauris posuere lectus ut ullamcorper vestibulum. Nullam quis lectus ac nibh semper ornare in ullamcorper leo. Sed vitae lectus quis magna euismod consectetur. Ut ac pharetra massa.""
    }
    ";

    private const string _json_200_without_tag_name = @"
    {
        ""url"": ""https://api.anyurl.com"",
        ""html_url"": ""https://anyurl.com/releases/tag/0.1.0"",
        ""body"":""Lorem ipsum dolor sit amet, consectetur adipiscing elit. Maecenas eu tellus dui. Quisque vestibulum tortor elit, nec ultricies odio viverra quis. In scelerisque, leo at tincidunt interdum, nibh neque tempus felis, eu maximus tellus ipsum quis ligula. Mauris posuere lectus ut ullamcorper vestibulum. Nullam quis lectus ac nibh semper ornare in ullamcorper leo. Sed vitae lectus quis magna euismod consectetur. Ut ac pharetra massa.""
    }
    ";
}