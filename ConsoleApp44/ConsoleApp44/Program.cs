// See https://aka.ms/new-console-template for more information
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Management;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

Console.WriteLine("Hello, World!");


var summary = BenchmarkRunner.Run(typeof(Program).Assembly);


[SimpleJob(RuntimeMoniker.Net80)]
//[RPlotExporter]
public class Test
{
    PropertyInfo namePropertyInfo;
    PropertyInfo surnamePropertyInfo;
    PropertyInfo agePropertyInfo;
    PropertyInfo pricePropertyInfo;

    Dictionary<string, PropertyHelper> properties;

    ExpressionSetter<Customer> exp;

    [Params(1000, 10000)]
    public int N;

    [GlobalSetup]
    public void Setup()
    {
        namePropertyInfo = typeof(Customer).GetProperty("Name");
        surnamePropertyInfo = typeof(Customer).GetProperty("Surname");
        agePropertyInfo = typeof(Customer).GetProperty("Age");
        pricePropertyInfo = typeof(Customer).GetProperty("Price");
  
        properties = PropertyHelper.GetProperties(typeof(Customer));

        exp = new ExpressionSetter<Customer>();
    }

    [Benchmark]
    public void TestReflection()
    {
        var c = new Customer();
        namePropertyInfo.SetValue(c, "test");
        surnamePropertyInfo.SetValue(c, "test");
        agePropertyInfo.SetValue(c, 15);
        pricePropertyInfo.SetValue(c, 1.65m);
    }

    [Benchmark]
    public void TestDelegate()
    {
        var c = new Customer();
        properties[nameof(Customer.Name)].Setter(c, "test");
        properties[nameof(Customer.Surname)].Setter(c, "test");
        properties[nameof(Customer.Age)].Setter(c, 12);
        properties[nameof(Customer.Price)].Setter(c, 1.65m);
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
public class ExpressionSetter<T>
{

    Dictionary<string, Action<T, object>> funcMap = new Dictionary<string, Action<T, object>>();

    public ExpressionSetter()
    {
        Register();
    }

    private void Register()
    {
        var type = typeof(T);
        var instanceParam = Expression.Parameter(type);
        var argumentParam = Expression.Parameter(typeof(object));

        funcMap = typeof(T).GetProperties().ToDictionary(x => x.Name, x =>
        {
            var expression = Expression.Lambda<Action<T, object>>(
                           Expression.Call(instanceParam, x.GetSetMethod(), Expression.Convert(argumentParam, x.PropertyType)),
                           instanceParam, argumentParam
                         ).Compile();

            return expression;
        });
    }

    public void Set(T instance, string propertyName, object value)
    {
        funcMap[propertyName](instance, value);
    }

    /*
    void ExpressionSet(string propName)
    {
        var type = typeof(T);

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
}


public class Customer
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public decimal Price { get; set; }
    public int Age { get; set; }
}


public class PropertyHelper
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyHelper>> Cache = new ConcurrentDictionary<Type, Dictionary<string, PropertyHelper>>();

    private static readonly MethodInfo CallInnerDelegateMethod = typeof(PropertyHelper).GetMethod(nameof(CallInnerDelegate), BindingFlags.NonPublic | BindingFlags.Static);
    private static readonly MethodInfo CallInnerDelegateMethod1 = typeof(PropertyHelper).GetMethod(nameof(CallInnerDelegate1), BindingFlags.NonPublic | BindingFlags.Static);

    private static Func<object, object> CallInnerDelegate<TClass, TResult>(Func<TClass, TResult> deleg)
        => instance => deleg((TClass)instance);

    private static Action<object, object> CallInnerDelegate1<TClass, TResult>(Action<TClass, TResult> deleg)
        => (instance, value) => deleg((TClass)(instance), (TResult)value);

    public string Name { get; set; }
    public Func<object, object> Getter { get; set; }
    public Action<object, object> Setter { get; set; }

    public static Dictionary<string, PropertyHelper> GetProperties(Type type)
    {
        if (!Cache.TryGetValue(type, out var values))
        {
            values = type.GetProperties().ToDictionary(x => x.Name, property =>
            {
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                var declaringClass = property.DeclaringType;
                var typeOfResult = property.PropertyType;


                // Func<Type, TResult>
                var getMethodDelegateType = typeof(Func<,>).MakeGenericType(declaringClass, typeOfResult);
                var setMethodDelegateType = typeof(Action<,>).MakeGenericType(declaringClass, typeOfResult);

                // c => c.Data
                var getMethodDelegate = getMethod.CreateDelegate(getMethodDelegateType);
                var setMethodDelegate = setMethod.CreateDelegate(setMethodDelegateType);

                // CallInnerDelegate<Type, TResult>
                var callInnerGenericMethodWithTypes = CallInnerDelegateMethod.MakeGenericMethod(declaringClass, typeOfResult);
                var callInnerGenericMethodWithTypes1 = CallInnerDelegateMethod1.MakeGenericMethod(declaringClass, typeOfResult);

                // Func<object, object>
                var result = (Func<object, object>)callInnerGenericMethodWithTypes.Invoke(null, new[] { getMethodDelegate });

                var setter = (Action<object, object>)callInnerGenericMethodWithTypes1.Invoke(null, new[] { setMethodDelegate });

                return new PropertyHelper
                {
                    Name = property.Name,
                    Getter = result,
                    Setter = setter
                };
            });

            Cache.TryAdd(type, values);
        }

        return values;
    }
    /*

    public static PropertyHelper[] GetProperties(Type type)
        => Cache
            .GetOrAdd(type, _ => type
            .GetProperties()
            .Select(property =>
            {
                var getMethod = property.GetMethod;
                var setMethod = property.SetMethod;
                var declaringClass = property.DeclaringType;
                var typeOfResult = property.PropertyType;


                // Func<Type, TResult>
                var getMethodDelegateType = typeof(Func<,>).MakeGenericType(declaringClass, typeOfResult);
                var setMethodDelegateType = typeof(Action<,>).MakeGenericType(declaringClass, typeOfResult);

                // c => c.Data
                var getMethodDelegate = getMethod.CreateDelegate(getMethodDelegateType);
                var setMethodDelegate = setMethod.CreateDelegate(setMethodDelegateType);

                // CallInnerDelegate<Type, TResult>
                var callInnerGenericMethodWithTypes = CallInnerDelegateMethod.MakeGenericMethod(declaringClass, typeOfResult);
                var callInnerGenericMethodWithTypes1 = CallInnerDelegateMethod1.MakeGenericMethod(declaringClass, typeOfResult);

                // Func<object, object>
                var result = (Func<object, object>)callInnerGenericMethodWithTypes.Invoke(null, new[] { getMethodDelegate });

                var setter = (Action<object, object>)callInnerGenericMethodWithTypes1.Invoke(null, new[] { setMethodDelegate });

                return new PropertyHelper
                {
                    Name = property.Name,
                    Getter = result,
                    Setter = setter
                };
            })
            .ToArray());
    */
}



