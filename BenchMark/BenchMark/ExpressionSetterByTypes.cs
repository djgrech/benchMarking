using System.Linq.Expressions;
using System.Reflection;

namespace BenchMark
{
    public class CustomExpressionMeta<T>
    {
        public Dictionary<string, Action<T, string>> stringFuncMap { get; set; }
        public Dictionary<string, Action<T, int>> intFuncMap { get; set; }
        public Dictionary<string, Action<T, decimal>> decimalFuncMap { get; set; }
    }

    public class CustomExpressionSetter<TEntity>
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

        public Dictionary<string, Action<TEntity, TType>> Register<TType>(PropertyInfo[] propertyInfos)
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

        public void Set(TEntity instance, string propertyName, string value)
        {
            customExpressionMeta.stringFuncMap[propertyName](instance, value);
        }

        public void Set(TEntity instance, string propertyName, decimal value)
        {
            customExpressionMeta.decimalFuncMap[propertyName](instance, value);
        }

        public void Set(TEntity instance, string propertyName, int value)
        {
            customExpressionMeta.intFuncMap[propertyName](instance, value);
        }


        /*
        private void RegisterStringProperties()
        {
            // Assuming all string properties have a setter that takes a string argument
            var type = typeof(T);
            var instanceParam = Expression.Parameter(type);
            var argumentParam = Expression.Parameter(typeof(string));

            stringFuncMap = type.GetProperties().Where(x => x.PropertyType == typeof(string))
                               .ToDictionary(x => x.Name, x =>
                               {
                                   var expression = Expression.Lambda<Action<T, string>>(
                               Expression.Call(instanceParam, x.GetSetMethod(), argumentParam),
                               instanceParam, argumentParam
                             ).Compile();

                                   return expression;
                               });
        }

        private void RegisterIntProperties()
        {
            // Assuming all int properties have a setter that takes an int argument
            var type = typeof(T);
            var instanceParam = Expression.Parameter(type);
            var argumentParam = Expression.Parameter(typeof(int));

            intFuncMap = type.GetProperties().Where(x => x.PropertyType == typeof(int))
                               .ToDictionary(x => x.Name, x =>
                               {
                                   var expression = Expression.Lambda<Action<T, int>>(
                               Expression.Call(instanceParam, x.GetSetMethod(), argumentParam),
                               instanceParam, argumentParam
                             ).Compile();

                                   return expression;
                               });
        }

        private void RegisterDoubleProperties()
        {
            // Assuming all decimal properties have a setter that takes a decimal argument
            var type = typeof(T);
            var instanceParam = Expression.Parameter(type);
            var argumentParam = Expression.Parameter(typeof(decimal));

            decimalFuncMap = type.GetProperties().Where(x => x.PropertyType == typeof(decimal))
                               .ToDictionary(x => x.Name, x =>
                               {
                                   var expression = Expression.Lambda<Action<T, decimal>>(
                               Expression.Call(instanceParam, x.GetSetMethod(), argumentParam),
                               instanceParam, argumentParam
                             ).Compile();

                                   return expression;
                               });
        }

        
        public void Set(T instance, string propertyName, string value)
        {
            stringFuncMap[propertyName](instance, value);
        }


        public void Set(T instance, string propertyName, int value)
        {
            intFuncMap[propertyName](instance, value);
        }

        public void Set(T instance, string propertyName, decimal value)
        {
            decimalFuncMap[propertyName](instance, value);
        }*/
    }
}
