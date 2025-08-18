using System.Collections;
using System.Collections.ObjectModel;

namespace CalDavNet;

public class SyncItemCollection : IEnumerable<SyncItem>
{
    public string SyncToken { get; }

    public IReadOnlyCollection<SyncItem> Items { get; }

    public SyncItemCollection(string syncToken, IEnumerable<SyncItem> items)
        : this(syncToken, items.ToList().AsReadOnly())
    {
    }

    public SyncItemCollection(string syncToken, ReadOnlyCollection<SyncItem> items)
    {
        SyncToken = syncToken;
        Items = items;
    }

    public IEnumerator<SyncItem> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
