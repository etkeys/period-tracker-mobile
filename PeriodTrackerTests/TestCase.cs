
namespace PeriodTrackerTests;

using Collection = Dictionary<string, object?>;

[Obsolete("Use TestCase<TParameters> instead.")]
public class TestCase(string name)
{
    private Collection _expected = new();
    private Collection _inputs = new();
    private Collection _setups = new();
    public Collection Expected => _expected;
    public Collection Inputs => _inputs;
    public Collection Setups => _setups;
    public string Name => name;

    public override string ToString() => Name;

    public TestCase WithExpected(string key, object? value){
        _expected.Add(key, value);
        return this;
    }

    public TestCase WithInput(string key, object? value){
        _inputs.Add(key, value);
        return this;
    }

    public TestCase WithSetup(string key, object? value){
        _setups.Add(key, value);
        return this;
    }
}

public class TestCase<TParameters>(string name, TParameters parameters)
{
    public string Name => name;

    public TParameters Parameters => parameters;

    public override string ToString() => Name;
}