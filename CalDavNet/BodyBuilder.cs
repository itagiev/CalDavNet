using System.Xml.Linq;

using static CalDavNet.Constants;

namespace CalDavNet;

public static class BodyBuilder
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
            foreach (var name in props) prop.Add(new XElement(name));
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
            foreach (var name in propfinds) propfind.Add(new XElement(name));
        }

        if (props.Count > 0)
        {
            var prop = new XElement(XNames.Prop);
            foreach (var name in props) prop.Add(new XElement(name));
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

    public static XDocument BuildReportBody(DateTime from, DateTime to)
    {
        var report = new XElement(XNames.CalendarQuery,
            new XAttribute(XNamespace.Xmlns + Constants.Dav.Prefix, Constants.Dav.Namespace),
            new XAttribute(XNamespace.Xmlns + Constants.Cal.Prefix, Constants.Cal.Namespace));

        var prop = new XElement(XNames.Prop,
            new XElement(XNames.GetETag),
            new XElement(XNames.CalendarData));

        var filter = new XElement(XNames.Filter);
        var vcalendarFilter = new XElement(XNames.CompFilter, new XAttribute("name", Constants.CompFilter.VCALENDAR));
        var veventFilter = new XElement(XNames.CompFilter, new XAttribute("name", CompFilter.VEVENT));
        var timeRange = new XElement(XNames.TimeRange,
            new XAttribute("start", from.ToString("yyyyMMdd'T'HHmmss'Z'")),
            new XAttribute("end", to.ToString("yyyyMMdd'T'HHmmss'Z'")));

        veventFilter.Add(timeRange);
        vcalendarFilter.Add(veventFilter);
        filter.Add(vcalendarFilter);

        report.Add(prop);
        report.Add(filter);

        return new XDocument(new XDeclaration("1.0", "UTF-8", null), report);
    }
}
