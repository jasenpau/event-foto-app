namespace EventFoto.Core.CalendarExport;

public interface ICalendarExportService
{
    public Task<string> ExportCalendarAsync();
}
