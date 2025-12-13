using Blazored.SessionStorage;
using System.Text;
using System.Text.Json;

namespace AnglingClubWebsite.Extensions
{
    public static class SessionStorageServiceExtension
    {
        public static async Task SaveItemEncrypted<T>(this ISessionStorageService sessionStorageService, string key, T item)
        {
            var itemJson = JsonSerializer.Serialize(item);
            var itemJsonBytes = Encoding.UTF8.GetBytes(itemJson);
            var base64Json = Convert.ToBase64String(itemJsonBytes);

            await sessionStorageService.SetItemAsync(key, base64Json);
        }

        public static async Task<T> ReadEncryptedItem<T>(this ISessionStorageService sessionStorageService, string key)
        {
            var storageItem = await sessionStorageService.GetItemAsync<string>(key);
            string? itemJson;

            if (storageItem == null)
            {
                return default!;
            }

            try
            {
                var itemJsonBytes = Convert.FromBase64String(storageItem!);
                itemJson = Encoding.UTF8.GetString(itemJsonBytes);

            }
            catch (System.FormatException)
            {
              // TODO Ang to Blazor Migration - not needed after migration complete
              // Not in base64 so attempting to read plain json
              itemJson = storageItem;
            }

            if (itemJson == null)
            {
              return default!;
            }

            var item = JsonSerializer.Deserialize<T>(itemJson, new JsonSerializerOptions
            {
              PropertyNameCaseInsensitive = true
            });

            return item!;

        }

    }
}
