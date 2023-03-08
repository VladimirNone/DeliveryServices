using DbManager.Data;
using Neo4jClient.Cypher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Neo4j
{
    internal static class Neo4jExtentions
    {
        public static ICypherFluentQuery<TModel> ChangeQueryForPagination<TModel>(this ICypherFluentQuery<TModel> query, string[] orderByProperty, int? skipCount = null, int? limitCount = null) where TModel : IModel
        {
            if (orderByProperty != null && orderByProperty.Length != 0)
                query = query.OrderBy(orderByProperty);

            query = query
                .Skip(skipCount)
                .Limit(limitCount);

            return query;
        }

        public static ICypherFluentQuery<T> ChangeQueryForPaginationAnonymousType<T>(this ICypherFluentQuery<T> query, string[] orderByProperty, int? skipCount = null, int? limitCount = null) where T : class
        {
            if (orderByProperty != null && orderByProperty.Length != 0)
                query = query.OrderBy(orderByProperty);

            query = query
                .Skip(skipCount)
                .Limit(limitCount);

            return query;
        }
    }
}
