using System.Xml.Linq;

namespace CalDavNet;

public static class BodyHelper
{
    public static XDocument BuildPropfind(IReadOnlyCollection<XName> props)
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Server.Prefix, Constants.Server.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Apple.Prefix, Constants.Apple.Namespace));

        if (props.Count > 0)
        {
            var prop = new XElement(XNames.Prop);
            foreach (var name in props)
                prop.Add(new XElement(name));
            propfind.Add(prop);
        }

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildPropfind(IReadOnlyCollection<XName> propfinds, IReadOnlyCollection<XName> props)
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Server.Prefix, Constants.Server.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Apple.Prefix, Constants.Apple.Namespace));

        if (propfinds.Count > 0)
        {
            foreach (var name in propfinds)
                propfind.Add(new XElement(name));
        }

        if (props.Count > 0)
        {
            var prop = new XElement(XNames.Prop);
            foreach (var name in props)
                prop.Add(new XElement(name));
            propfind.Add(prop);
        }

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildAllPropPropfind()
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XElement(XNames.AllProp));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildSyncCollection(string? syncToken = null)
    {
        var syncCollection = new XElement(XNames.SyncCollection,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XElement(XNames.SyncToken, syncToken),
            new XElement(XNames.SyncLevel, 1),
            new XElement(XNames.Prop, new XElement(XNames.GetEtag)));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), syncCollection);
    }

    public static XDocument BuildCurrentUserPrincipalPropfind()
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XElement(XNames.CurrentUserPrincipal));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildCalendarQuery(XElement filter)
    {
        var calendarQuery = new XElement(XNames.CalendarQuery,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace));

        var prop = new XElement(XNames.Prop,
            new XElement(XNames.GetEtag),
            new XElement(XNames.CalendarData));

        calendarQuery.Add(prop);
        calendarQuery.Add(filter);

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), calendarQuery);
    }

    public static XDocument BuildCalendarMultiget(params string[] hrefs)
        => BuildCalendarMultiget(hrefs.AsEnumerable());

    public static XDocument BuildCalendarMultiget(IEnumerable<string> hrefs)
    {
        var multiget = new XElement(XNames.CalendarMultiget,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace));

        var prop = new XElement(XNames.Prop,
            new XElement(XNames.GetEtag),
            new XElement(XNames.CalendarData));

        multiget.Add(prop);

        foreach (var href in hrefs)
            multiget.Add(new XElement(XNames.Href, href));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), multiget);
    }

    public static XDocument BuildMkcalendar(string displayName, string supportedComponent, string? description = null,
        string? color = null)
    {
        var supportedCalendarComponentSet = new XElement(XNames.SupportedCalendarComponentSet,
            new XElement(XNames.Comp, new XAttribute("name", supportedComponent)));

        var prop = new XElement(XNames.Prop,
            new XElement(XNames.DisplayName, displayName),
            supportedCalendarComponentSet);

        if (!string.IsNullOrEmpty(description))
            prop.Add(new XElement(XNames.CalendarDescription, description));

        if (!string.IsNullOrEmpty(color))
            prop.Add(new XElement(XNames.CalendarColor, color));

        var mkcalendar = new XElement(XNames.Mkcalendar,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace),
            new XElement(XNames.Set, prop));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), mkcalendar);
    }

    public static XDocument BuildPropertyUpdate(string? displayName = null, string? description = null,
        string? color = null)
    {
        var prop = new XElement(XNames.Prop);

        if (!string.IsNullOrEmpty(displayName))
            prop.Add(new XElement(XNames.DisplayName, displayName));

        if (!string.IsNullOrEmpty(description))
            prop.Add(new XElement(XNames.CalendarDescription, description));

        if (!string.IsNullOrEmpty(color))
            prop.Add(new XElement(XNames.CalendarColor, color));

        var propertyUpdate = new XElement(XNames.PropertyUpdate,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace),
            new XElement(XNames.Set, prop));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propertyUpdate);
    }
}
