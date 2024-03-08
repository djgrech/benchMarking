using System.Linq.Expressions;

namespace BenchMark
{
    public class ExpressionSetter<TEntity>
    {
        Dictionary<string, Action<TEntity, object>> funcMap = new Dictionary<string, Action<TEntity, object>>();

        public ExpressionSetter()
        {
            Register();
        }

        public void Set(TEntity instance, string propertyName, object value)
            => funcMap[propertyName](instance, value);

        private void Register()
        {
            var type = typeof(TEntity);
            var instanceParam = Expression.Parameter(type);
            var argumentParam = Expression.Parameter(typeof(object));

            funcMap = type.GetProperties().ToDictionary(x => x.Name, x =>
            {
                var expression = Expression.Lambda<Action<TEntity, object>>(
                    Expression.Call(instanceParam, x.GetSetMethod(), Expression.Convert(argumentParam, x.PropertyType)),
                    instanceParam, argumentParam
                ).Compile();

                return expression;
            });
        }
    }
}
