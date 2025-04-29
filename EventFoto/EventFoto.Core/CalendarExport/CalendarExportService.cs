using System.Text;
using EventFoto.Data.BlobStorage;
using EventFoto.Data.Models;
using EventFoto.Data.Repositories;

namespace EventFoto.Core.CalendarExport;

public class CalendarExportService : ICalendarExportService
{
    public IEventRepository _eventRepository;

    public CalendarExportService(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<string> ExportCalendarAsync()
    {
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddYears(1);
        var events = await _eventRepository.GetAllEventsInDatesAsync(startDate, endDate);
        return ExportToIcs(events);
    }

    private static string ExportToIcs(List<Event> events)
    {
        var ical = new StringBuilder();
        ical.AppendLine("BEGIN:VCALENDAR");
        ical.AppendLine("VERSION:2.0");
        ical.AppendLine("PRODID:-//YourCompany//YourApp//EN");

        foreach (var e in events)
        {
            ical.AppendLine("BEGIN:VEVENT");
            ical.AppendLine($"UID:{e.Id}@yourapp.com");
            ical.AppendLine($"DTSTAMP:{e.CreatedOn.ToUniversalTime():yyyyMMddTHHmmssZ}");
            ical.AppendLine($"DTSTART:{e.StartDate.ToUniversalTime():yyyyMMddTHHmmssZ}");

            if (e.EndDate.HasValue)
                ical.AppendLine($"DTEND:{e.EndDate.Value.ToUniversalTime():yyyyMMddTHHmmssZ}");

            ical.AppendLine($"SUMMARY:{EscapeText(e.Name)}");
            ical.AppendLine($"DESCRIPTION:{EscapeText(e.Note)}");
            ical.AppendLine($"LOCATION:{EscapeText(e.Location)}");
            ical.AppendLine("END:VEVENT");
        }

        ical.AppendLine("END:VCALENDAR");

        return ical.ToString();
    }

    private static string EscapeText(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";
        return input.Replace("\\", "\\\\")
            .Replace(";", "\\;")
            .Replace(",", "\\,")
            .Replace("\n", "\\n")
            .Replace("\r", "");
    }
}
