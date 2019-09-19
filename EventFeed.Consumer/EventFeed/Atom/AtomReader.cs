using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;

namespace EventFeed.Consumer.EventFeed.Atom
{
    internal class AtomReader
    {
        public async Task<AtomPage> ReadAtomPage(Stream stream)
        {
            using (var xmlReader = XmlReader.Create(stream, new XmlReaderSettings { Async = true }))
            {
                var feedReader = new AtomFeedReader(xmlReader);
                var entries = new List<AtomEntry>();
                Uri nextPageUri = null, previousPageUri = null, realTimeNotificationUri = null;

                while (await feedReader.Read())
                {
                    switch (feedReader.ElementType)
                    {
                        case SyndicationElementType.Item:
                            var item = (Microsoft.SyndicationFeed.Atom.AtomEntry) await feedReader.ReadItem();

                            var atomEntry = new AtomEntry(
                                id: item.Id,
                                published: item.Published,
                                contentType: item.ContentType,
                                payload: item.Description
                            );
                            entries.Insert(0, atomEntry);
                            break;

                        case SyndicationElementType.Link:
                            ISyndicationLink link = await feedReader.ReadLink();

                            switch (link.RelationshipType)
                            {
                                case "next-archive" when nextPageUri == null:
                                    nextPageUri = link.Uri;
                                    break;
                                case "prev-archive" when previousPageUri == null:
                                    previousPageUri = link.Uri;
                                    break;
                                case "notifications" when realTimeNotificationUri == null:
                                    realTimeNotificationUri = link.Uri;
                                    break;
                            }

                            break;
                    }
                }

                return new AtomPage(
                    nextArchivePageUri: nextPageUri,
                    previousArchivePageUri: previousPageUri,
                    realTimeNotificationUri: realTimeNotificationUri,
                    entries: entries
                );
            }
        }
    }
}
