using System.IO;
using System.IO.IsolatedStorage;
using EventFeed.Producer.Clicks;

namespace EventFeed.Producer.Infrastructure
{
    internal class IsolatedStorageClickStorage: IClickStorage
    {
        public void IncrementClickCount()
        {
            lock (this)
            {
                int currentClickCount = GetClickCountInternal();

                using (var writer = new StreamWriter(_storage.OpenFile(FileName, FileMode.Create, FileAccess.Write)))
                {
                    writer.Write(currentClickCount + 1);
                }
            }
        }

        public int GetClickCount()
        {
            lock (this)
            {
                return GetClickCountInternal();
            }
        }

        private int GetClickCountInternal()
        {
            if (!_storage.FileExists("clicks.dat"))
                return 0;

            using (var reader = new StreamReader(_storage.OpenFile(FileName, FileMode.Open, FileAccess.Read)))
            {
                return int.Parse(reader.ReadToEnd());
            }
        }

        private readonly IsolatedStorageFile _storage = IsolatedStorageFile.GetUserStoreForApplication();

        private const string FileName = "clicks.dat";
    }
}
