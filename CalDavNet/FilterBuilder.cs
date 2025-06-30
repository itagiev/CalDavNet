using System.Xml.Linq;

namespace CalDavNet;

public class FilterBuilder
{
    private readonly XElement _filter = new XElement(XNames.Filter);
    private readonly XElement _vcalendar = new XElement(XNames.CompFilter, new XAttribute("name", Constants.CompFilter.VCALENDAR));

    public FilterBuilder()
    {
        _filter.Add(_vcalendar);
    }

    public XElement ToXElement => _filter;

    public FilterBuilder AddCompFilter(XElement compFilter)
    {
        _vcalendar.Add(compFilter);
        return this;
    }

    public CompFilterBuilder AddCompFilter(string compName)
    {
        var compFilterBuilder = new CompFilterBuilder(compName);
        _vcalendar.Add(compFilterBuilder.ToXElement);
        return compFilterBuilder;
    }
}

public class CompFilterBuilder
{
    private readonly XElement _compFilter = null!;

    public CompFilterBuilder(string compName)
    {
        _compFilter = new XElement(XNames.CompFilter, new XAttribute("name", compName));
    }

    public XElement ToXElement => _compFilter;

    public CompFilterBuilder AddTimeRange(DateTime start, DateTime end)
    {
        var timeRange = new XElement(XNames.TimeRange,
            new XAttribute("start", start.ToString("yyyyMMdd'T'HHmmss'Z'")),
            new XAttribute("end", end.ToString("yyyyMMdd'T'HHmmss'Z'")));

        _compFilter.Add(timeRange);

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propName"></param>
    /// <param name="value"></param>
    /// <param name="negate"></param>
    /// <param name="collation"></param>
    /// <returns></returns>
    public CompFilterBuilder AddTextMatch(string propName, string value,
        bool negate = false, string? collation = null)
    {
        var textMatch = new XElement(XNames.TextMatch, value);

        if (negate)
            textMatch.SetAttributeValue("negate", "yes");

        if (!string.IsNullOrWhiteSpace(collation))
            textMatch.SetAttributeValue("collation", collation);

        var propFilter = new XElement(XNames.PropFilter, new XAttribute("name", propName), textMatch);
        _compFilter.Add(propFilter);

        return this;
    }

    // TODO: add more prop filters
}
