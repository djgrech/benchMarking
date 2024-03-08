using System.Linq.Expressions;

namespace BenchMark
{
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
    }

}
