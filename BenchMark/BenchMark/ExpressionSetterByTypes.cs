using System.Linq.Expressions;
using System.Reflection;

namespace BenchMark
{
    public class CustomExpressionMeta<TEntity>
    {
        public Dictionary<string, Action<TEntity, string>> stringFuncMap { get; set; }
        public Dictionary<string, Action<TEntity, int>> intFuncMap { get; set; }
        public Dictionary<string, Action<TEntity, decimal>> decimalFuncMap { get; set; }
    }

    public interface ICustomExpressionSetter<in TEntity>
    {
        void Set(TEntity instance, string propertyName, string value);
        void Set(TEntity instance, string propertyName, decimal value);
        void Set(TEntity instance, string propertyName, int value);
    }

    public class CustomExpressionSetter<TEntity> : ICustomExpressionSetter<TEntity>
    {
        private CustomExpressionMeta<TEntity> customExpressionMeta = new();

        public CustomExpressionSetter()
        {
            customExpressionMeta = new();
            var properties = typeof(TEntity).GetProperties();

            customExpressionMeta.stringFuncMap = Register<string>(properties);
            customExpressionMeta.intFuncMap = Register<int>(properties);
            customExpressionMeta.decimalFuncMap = Register<decimal>(properties);
        }

        public void Set(TEntity instance, string propertyName, string value)
            => customExpressionMeta.stringFuncMap[propertyName](instance, value);

        public void Set(TEntity instance, string propertyName, decimal value)
            => customExpressionMeta.decimalFuncMap[propertyName](instance, value);

        public void Set(TEntity instance, string propertyName, int value)
            => customExpressionMeta.intFuncMap[propertyName](instance, value);

        private Dictionary<string, Action<TEntity, TType>> Register<TType>(PropertyInfo[] propertyInfos)
        {
            var entityType = typeof(TEntity);
            var propType = typeof(TType);

            var instanceParam = Expression.Parameter(entityType);
            var argumentParam = Expression.Parameter(propType);

            return propertyInfos.Where(x => x.PropertyType == propType)
                .ToDictionary(x => x.Name, x =>
                {
                    var expression = Expression.Lambda<Action<TEntity, TType>>(
                        Expression.Call(instanceParam, x.GetSetMethod(), argumentParam),
                        instanceParam, argumentParam
                    ).Compile();

                    return expression;
                });
        }

    }
}
