
namespace PeriodTracker;

public static class Extensions
{
    public static DataTypeAttribute GetDataTypeAttribute(this AppStateProperty source) =>
        (DataTypeAttribute)source.GetType()
            .GetMember(source.ToString())[0]
            .GetCustomAttributes(typeof(DataTypeAttribute), false)[0];
}