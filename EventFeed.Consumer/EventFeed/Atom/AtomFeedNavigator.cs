using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventFeed.Consumer.EventFeed.Atom
{
    internal class AtomFeedNavigator
    {
        public async Task<IReadOnlyCollection<AtomEntry>> GetEntriesSinceAsync(string? entryId)
        {
            var entries = new List<AtomEntry>();
            Uri? currentUri = _initialUri;

            do
            {
                var page = await ReadAtomPageAsync(currentUri);
                entries.InsertRange(0, page.Entries);

                currentUri = page.PreviousArchivePageUri;
            }
            while (currentUri != null && DoesNotContainLastKnownEntry());

            int indexOfLastKnownEntry = entries.FindIndex(e => e.Id == entryId);
            if (indexOfLastKnownEntry == -1 && !string.IsNullOrEmpty(entryId))
                throw new Exception($"Could not find last-known entry '{entryId}' in feed at {_initialUri}.");
            if (indexOfLastKnownEntry != -1)
                entries.RemoveRange(0, indexOfLastKnownEntry + 1);

            return entries;

            bool DoesNotContainLastKnownEntry() => entries.All(e => e.Id != entryId);
        }

        private async Task<AtomPage> ReadAtomPageAsync(Uri uri)
        {
            var httpClient = _httpClientFactory();
            var reader = new AtomReader();

            using (var stream = await httpClient.GetStreamAsync(uri))
            {
                return await reader.ReadAtomPage(stream);
            }
        }

        public AtomFeedNavigator(
            Uri initialUri,
            Func<HttpClient> httpClientFactory
        )
        {
            _initialUri = initialUri;
            _httpClientFactory = httpClientFactory;
        }

        private readonly Uri _initialUri;
        private readonly Func<HttpClient> _httpClientFactory;
    }
}
