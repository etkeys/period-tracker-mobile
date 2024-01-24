
namespace PeriodTrackerTests;

public class BaseTest
{

    protected static IEnumerable<object[]> BundleTestCases(params TestCase[] testCases) =>
        testCases.Select(tc => new object[]{tc});

}