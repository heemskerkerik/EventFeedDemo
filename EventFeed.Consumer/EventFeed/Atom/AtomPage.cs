using System;
using System.Collections.Generic;

namespace EventFeed.Consumer.EventFeed.Atom
{
    internal class AtomPage
    {
        public Uri NextArchivePageUri { get; }
        public Uri PreviousArchivePageUri { get; }
        public IReadOnlyCollection<AtomEntry> Entries { get; }

        public AtomPage(
            Uri nextArchivePageUri, 
            Uri previousArchivePageUri, 
            IReadOnlyCollection<AtomEntry> entries
        )
        {
            NextArchivePageUri = nextArchivePageUri;
            PreviousArchivePageUri = previousArchivePageUri;
            Entries = entries;
        }
    }
}