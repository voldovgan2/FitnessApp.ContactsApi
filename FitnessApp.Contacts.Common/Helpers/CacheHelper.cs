using System.Threading.Tasks;
using FitnessApp.Common.Serializer;
using Microsoft.Extensions.Caching.Distributed;

namespace FitnessApp.Contacts.Common.Helpers;

public static class CacheHelper
{
    public static Task SaveData<T>(IDistributedCache cache, string key, T value)
    {
        return cache.SetAsync(key, JsonConvertHelper.SerializeToBytes(value, []));
    }

    public static async Task<T> LoadData<T>(IDistributedCache cache, string key)
    {
        return JsonConvertHelper.DeserializeFromBytes<T>(await cache.GetAsync(key));
    }
}
