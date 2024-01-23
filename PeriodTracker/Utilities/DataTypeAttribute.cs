
namespace PeriodTracker;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class DataTypeAttribute : Attribute
{
    public DataTypeAttribute(Type type){
        Value = type;
    }

    public Type Value {get;}
}
