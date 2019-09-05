using System.IO;
using System.IO.IsolatedStorage;
using EventFeed.Consumer.EventFeed;

namespace EventFeed.Consumer.Infrastructure
{
    internal class IsolatedStorageKnownEventStorage: IKnownEventStorage
    {
        public string GetLastKnownEventId()
        {
            lock (this)
            {
                if (!_storage.FileExists(FileName))
                    return null;

                using (var reader = new StreamReader(_storage.OpenFile(FileName, FileMode.Open, FileAccess.Read)))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public void StoreLastKnownEventId(string id)
        {
            lock (this)
            {
                using (var writer = new StreamWriter(_storage.OpenFile(FileName, FileMode.Create, FileAccess.Write)))
                {
                    writer.Write(id);
                }
            }
        }
        
        private readonly IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();

        private const string FileName = "knownevent.dat";
    }
}
