namespace CalDavNet;

[Flags]
public enum CalendarComponent : int
{
    None = 0,
    VEVENT = 1,
    VTODO = 1 << 1,
    VJOURNAL = 1 << 2,
    VFREEBUSY = 1 << 3,
    VALARM = 1 << 4
}
