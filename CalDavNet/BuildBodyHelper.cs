using System.Xml.Linq;

namespace CalDavNet;

public static class BuildBodyHelper
{
    public static XDocument BuildPropfindBody(IReadOnlyCollection<XName> props)
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

    public static XDocument BuildPropfindBody(IReadOnlyCollection<XName> propfinds, IReadOnlyCollection<XName> props)
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

    public static XDocument BuildAllPropPropfindBody()
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XElement(XNames.AllProp));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildCurrentUserPrincipalPropfindBody()
    {
        var propfind = new XElement(XNames.Propfind,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XElement(XNames.CurrentUserPrincipal));

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), propfind);
    }

    public static XDocument BuildCalendarQueryBody(XElement filter)
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

    public static XDocument BuildCalendarMultigetBody(params string[] hrefs)
        => BuildCalendarMultigetBody(hrefs.AsEnumerable());

    public static XDocument BuildCalendarMultigetBody(IEnumerable<string> hrefs)
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
}
