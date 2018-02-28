using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace GZCDCPay.Services
{
    public class SignatureService
    {
        private readonly IApiKeyProvider IDKeyMap;

        public SignatureService(SignatureServiceOptions options)
        {
            this.IDKeyMap = options.ApiKeyProvider;

        }
        public string CalculateSignature(IEnumerable<KeyValuePair<string, string>> items, string appId)
        {
            if (!IDKeyMap.ContainsKey(appId))
            {
                throw new KeyNotFoundException($"ApiKey for AppId {appId} not found.");
            }
            var apiKey = IDKeyMap[appId];

            string arguments = items.Where(x => !String.IsNullOrEmpty(x.Value))
                .OrderBy(x => x.Key)
                .Aggregate(new StringBuilder(),
                    (sb, next) => sb.Append($"&{next.Key}={next.Value}"),
                    result => result.Append($"&key={apiKey}").ToString().TrimStart('&'));
            var hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(arguments));
            var signature = BitConverter.ToString(hash).Replace("-", string.Empty);
            return signature;
        }

        public string GenerateNonceStr()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }

    }


    public interface IApiKeyProvider
    {
        bool ContainsKey(string key);
        string GetValue(string key);
        string this[string key] { get; }
    }

    public class IDKeyDbProvider<TEntity> : IApiKeyProvider
                 where TEntity : class
    {
        private readonly DbSet<TEntity> keyMap;
        private readonly Func<TEntity, string> keySelector;
        private readonly Func<TEntity, string> valueSelector;
        public IDKeyDbProvider(DbSet<TEntity> keyMap, Func<TEntity, string> keySelector, Func<TEntity, string> valueSelector)
        {
            this.keyMap = keyMap;
            this.keySelector = keySelector;
            this.valueSelector = valueSelector;
        }
        public bool ContainsKey(string key)
        {
            return keyMap.Count(x => keySelector.Invoke(x) == key) > 0;
        }

        public string GetValue(string key)
        {
            return valueSelector.Invoke(keyMap.AsNoTracking().Single(x => keySelector.Invoke(x) == key));
        }

        public string this[string key]
        {
            get
            {
                return GetValue(key);
            }
        }
    }

    public class IDKeyInMemoryProvider : IApiKeyProvider
    {
        private IDictionary<string, string> map;
        public IDKeyInMemoryProvider(IDictionary<string, string> map)
        {
            this.map = map;
        }

        public string this[string key]
        {
            get
            {
                return map[key];
            }
        }

        public bool ContainsKey(string key)
        {
            return map.ContainsKey(key);
        }

        public string GetValue(string key)
        {
            return map[key];
        }
    }

    public class SignatureServiceOptions
    {
        public IApiKeyProvider ApiKeyProvider { get; set; }
    }

    public class SignatureServiceOptionsBuilder
    {
        private readonly IServiceProvider provider;
        private IApiKeyProvider apiKeyProvider;
        private Dictionary<string, string> map = new Dictionary<string, string>();

        public SignatureServiceOptionsBuilder(IServiceProvider provider)
        {
            this.provider = provider;
        }
        public SignatureServiceOptionsBuilder UseDbContext<TContext, TEntity>(
            Func<TContext, DbSet<TEntity>> entitySelector,
            Func<TEntity, string> keySelector,
            Func<TEntity, string> valueSelector)
                                    where TContext : DbContext
                                    where TEntity : class
        {
            var context = provider.GetService<TContext>();
            //context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            var dbSet = entitySelector.Invoke(context);
            this.apiKeyProvider = new IDKeyDbProvider<TEntity>(dbSet, keySelector, valueSelector);
            return this;
        }
        public IDictionary<string, string> UseInMemoryMap(IDictionary<string, string> map)
        {
            foreach (var item in map)
            {
                this.map.Add(item.Key, item.Value);
            }
            return this.map;
        }
        public IDictionary<string, string> UseInMemoryMap()
        {
            return this.map;
        }

        public SignatureServiceOptions Options
        {
            get
            {
                if (this.apiKeyProvider != null)
                {
                    return new SignatureServiceOptions() { ApiKeyProvider = this.apiKeyProvider };
                }
                else
                {
                    return new SignatureServiceOptions() { ApiKeyProvider = new IDKeyInMemoryProvider(this.map) };
                }
            }
        }
    }




    public static class SignatureServiceExtensions
    {
        public static IServiceCollection AddSignatureService(this IServiceCollection coll, Action<SignatureServiceOptionsBuilder> optionsBuilder)
        {
            coll.AddScoped<SignatureService>(provider =>
            {
                SignatureServiceOptionsBuilder builder = new SignatureServiceOptionsBuilder(provider);
                optionsBuilder.Invoke(builder);
                return new SignatureService(builder.Options);
            });

            return coll;
        }
    }
}