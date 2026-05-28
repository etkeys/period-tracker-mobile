using System.Net;
using Moq;
using Moq.Protected;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, ClassData(typeof(GetLatestVersionTestsData))]
    public async Task GetLatestVersionTests(TestCase<GetLatestVersionTestsData.TestParameters> test){

        SetupHttpClientFactoryMock(test.Parameters.Inputs.HttpResponseMessage);

        using var actor = new UpdateService(_httpClientFactoryMock.Object, _dbContextProviderMock.Object);
        var actVersion = await actor.GetLatestVersion();

        var expVersion = test.Parameters.Expected.Version;

        Assert.Equal(expVersion, actVersion);
    }

    private void SetupHttpClientFactoryMock(HttpResponseMessage setupResponseMessage){
        // ref: https://stackoverflow.com/a/44028625
        var mh = new Mock<HttpMessageHandler>();
        mh.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(setupResponseMessage);

        _httpClientFactoryMock.Setup(m =>
            m.CreateClient(It.IsAny<string>())).Returns(new HttpClient(mh.Object));
    }

    public class GetLatestVersionTestsData: IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);

        private static IEnumerable<object[]> TestCases()
        {
            yield return new []{
                new TestCase<TestParameters>("200 with correct data",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(_json_200_with_correct_data)
                        }
                    },
                    new ExpectedResults{
                        Version = new Version("0.1.0")
                    }
                ))};

            yield return new []{
                new TestCase<TestParameters>("200 but no json",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(string.Empty)
                        }
                    },
                    new ExpectedResults{
                        Version = null
                    }
                ))};

            yield return new []{
                 new TestCase<TestParameters>("200 but tag_name is not present",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(_json_200_without_tag_name)
                        }
                    },
                    new ExpectedResults{
                        Version = null
                    }
                ))};

            yield return new []{
                 new TestCase<TestParameters>("200 but tag_name is not a version string",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(_json_200_tag_name_is_not_version)
                        }
                    },
                    new ExpectedResults{
                        Version = null
                    }
                ))};

             yield return new []{
                 new TestCase<TestParameters>("400",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.BadRequest,
                            Content = new StringContent(string.Empty)
                        }
                    },
                    new ExpectedResults{
                        Version = null
                    }
                ))};

             yield return new []{
                 new TestCase<TestParameters>("404",
                new TestParameters(
                    new Inputs{
                        HttpResponseMessage = new HttpResponseMessage{
                            StatusCode = HttpStatusCode.NotFound,
                            Content = new StringContent(string.Empty)
                        }
                    },
                    new ExpectedResults{
                        Version = null
                    }
                ))};

        }

        public IEnumerator<object[]> GetEnumerator() => TestCases().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public Version? Version {get; init;}
        }

        public class Inputs
        {
            public required HttpResponseMessage HttpResponseMessage {get; init;}
        }

    }

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