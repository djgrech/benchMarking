using System.Collections.Concurrent;
using System.Reflection;

namespace BenchMark
{
    public class PropertyHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyHelper>> Cache = new();

        private static readonly MethodInfo CallInnerDelegateMethodGetter = typeof(PropertyHelper).GetMethod(nameof(CallInnerDelegateGetter), BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly MethodInfo CallInnerDelegateMethodSetter = typeof(PropertyHelper).GetMethod(nameof(CallInnerDelegateSetter), BindingFlags.NonPublic | BindingFlags.Static);

        private static Func<object, object> CallInnerDelegateGetter<TClass, TResult>(Func<TClass, TResult> deleg)
            => instance => deleg((TClass)instance);

        private static Action<object, object> CallInnerDelegateSetter<TClass, TResult>(Action<TClass, TResult> deleg)
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
                    var callInnerGenericMethodWithTypes = CallInnerDelegateMethodGetter.MakeGenericMethod(declaringClass, typeOfResult);
                    var callInnerGenericMethodWithTypes1 = CallInnerDelegateMethodSetter.MakeGenericMethod(declaringClass, typeOfResult);

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
    }
}
