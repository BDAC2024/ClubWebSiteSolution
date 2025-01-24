using Blazored.LocalStorage;
using System.Text;
using System.Text.Json;

namespace AnglingClubWebsite.Extensions
{
    public static class LocalStorageServiceExtension
    {
        public static async Task SaveItemEncrypted<T>(this ILocalStorageService localStorageService, string key, T item)
        {
            var itemJson = JsonSerializer.Serialize(item);
            var itemJsonBytes = Encoding.UTF8.GetBytes(itemJson);
            var base64Json = Convert.ToBase64String(itemJsonBytes);

            await localStorageService.SetItemAsync(key, base64Json);
        }

        public static async Task<T> ReadEncryptedItem<T>(this ILocalStorageService localStorageService, string key)
        {
            var base64Json = await localStorageService.GetItemAsync<string>(key);
            
            if (base64Json == null)
            {
                return default!;
            }

            var itemJsonBytes = Convert.FromBase64String(base64Json!);
            var itemJson = Encoding.UTF8.GetString(itemJsonBytes);

            var item = JsonSerializer.Deserialize<T>(itemJson);

            return item!;
        }

    }
}
