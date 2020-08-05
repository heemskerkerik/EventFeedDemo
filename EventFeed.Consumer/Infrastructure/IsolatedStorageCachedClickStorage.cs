using System.IO;
using System.IO.IsolatedStorage;
using EventFeed.Consumer.Events;

namespace EventFeed.Consumer.Infrastructure
{
    internal class IsolatedStorageCachedClickStorage: ICachedClickStorage
    {
        public int GetClickCount()
        {
            lock (this)
            {
                if (!_storage.FileExists(FileName))
                    return 0;

                using var reader = new StreamReader(_storage.OpenFile(FileName, FileMode.Open, FileAccess.Read));
                return int.Parse(reader.ReadToEnd());
            }
        }

        public void StoreClickCount(int count)
        {
            lock (this)
            {
                using var writer = new StreamWriter(_storage.OpenFile(FileName, FileMode.Create, FileAccess.Write));
                writer.Write(count);
            }
        }
        
        private readonly IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();

        private const string FileName = "clicks.dat";
    }
}
