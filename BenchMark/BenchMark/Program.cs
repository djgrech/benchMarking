// See https://aka.ms/new-console-template for more information
using BenchMark;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Reflection;

var summary = BenchmarkRunner.Run(typeof(Program).Assembly);

[SimpleJob(RuntimeMoniker.Net80)]
//[RPlotExporter]
public class Test
{
    // for reflection
    Dictionary<string, PropertyInfo> properties;

    // dictionary for PropertyHelper
    Dictionary<string, PropertyHelper> propertyProperties;

    // for Expression Setter
    ExpressionSetter<Customer> exp;

    [Params(1000, 10000)] public int N;

    [GlobalSetup]
    public void Setup()
    {
        // reflection
        properties = typeof(Customer).GetProperties().ToDictionary(x => x.Name, x => x);

        // delegate
        propertyProperties = PropertyHelper.GetProperties(typeof(Customer));

        // expression
        exp = new ExpressionSetter<Customer>();
    }

    [Benchmark]
    public void TestReflection()
    {
        var c = new Customer();
        properties[nameof(Customer.Name)].SetValue(c, "test");
        properties[nameof(Customer.Surname)].SetValue(c, "test");
        properties[nameof(Customer.Age)].SetValue(c, 15);
        properties[nameof(Customer.Price)].SetValue(c, 1.65m);
    }

    [Benchmark]
    public void TestDelegate()
    {
        var c = new Customer();
        propertyProperties[nameof(Customer.Name)].Setter(c, "test");
        propertyProperties[nameof(Customer.Surname)].Setter(c, "test");
        propertyProperties[nameof(Customer.Age)].Setter(c, 12);
        propertyProperties[nameof(Customer.Price)].Setter(c, 1.65m);
    }

    [Benchmark]
    public void TestExpression()
    {
        var c = new Customer();
        exp.Set(c, "Name", "test");
        exp.Set(c, "Surname", "test");
        exp.Set(c, "Age", 12);
        exp.Set(c, "Price", 1.65m);
    }
}



/*
TestReflection();
TestExpression();
TestDelegate();
*/

//ExpressionSet();


/*foreach (var prop in props)
{
    Console.WriteLine(prop.Getter(c));
}
*/
/*
void TestReflection()
{
    var c = new Customer();
    var namePropertyInfo = typeof(Customer).GetProperty("Name");
    var surnamePropertyInfo = typeof(Customer).GetProperty("Surname");
    var agePropertyInfo = typeof(Customer).GetProperty("Age");
    var pricePropertyInfo = typeof(Customer).GetProperty("Price");

    var s = new Stopwatch();
    s.Start();

    for (int i = 0; i < 1000000; i++)
    {
        namePropertyInfo.SetValue(c, "test");
        surnamePropertyInfo.SetValue(c, "test");
        agePropertyInfo.SetValue(c, 15);
        pricePropertyInfo.SetValue(c, 1.65m);
    }
    s.Stop();

    Console.WriteLine($"Reflection: {s.ElapsedMilliseconds}");
}

void TestDelegate()
{
    var properties = PropertyHelper.GetProperties(typeof(Customer));
    var c = new Customer();
    var s = new Stopwatch();
    s.Start();

    for (int i = 0; i < 1000000; i++)
    {
        properties[nameof(Customer.Name)].Setter(c, "test");
        properties[nameof(Customer.Surname)].Setter(c, "test");
        properties[nameof(Customer.Age)].Setter(c, 12);
        properties[nameof(Customer.Price)].Setter(c, 1.65m);
    }
    s.Stop();

    Console.WriteLine($"Delegate: {s.ElapsedMilliseconds}");
}


void TestExpression()
{
    var exp = new ExpressionSetter<Customer>();
    var c = new Customer();
    var s = new Stopwatch();
    s.Start();

    for (int i = 0; i < 1000000; i++)
    {
        exp.Set(c, "Name", "test");
        exp.Set(c, "Surname", "test");
        exp.Set(c, "Age", 12);
        exp.Set(c, "Price", 1.65m);
        //setter(c, "test");
    }
    s.Stop();

    Console.WriteLine($"Expression: {s.ElapsedMilliseconds}");
}

void ExpressionSet()
{
    var instance = new Customer();
    var type = instance.GetType();

    var instanceParam = Expression.Parameter(type);
    var argumentParam = Expression.Parameter(typeof(object));

    var propertyInfo = type.GetProperty("Name");

    var expression = Expression.Lambda<Action<Customer, object>>(
                   Expression.Call(instanceParam, propertyInfo.GetSetMethod(), Expression.Convert(argumentParam, propertyInfo.PropertyType)),
                   instanceParam, argumentParam
                 ).Compile();

    var s = new Stopwatch();
    s.Start();

    for (int i = 0; i < 1000000; i++)
    {
        expression(instance, "TEST");
    }
    s.Stop();

    var elapsed = s.ElapsedMilliseconds;

    elapsed = 0;
}
*/