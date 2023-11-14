//--------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet.Examples;

/// <summary>
/// The main program.
/// </summary>
public static class Program
{
    /// <summary>
    /// The main method.
    /// </summary>
    public static async Task Main()
    {
        // Create client.
        //Only works with basic auth
        //Test with baikal => http(s)://server_url:server_port/dav.php/calendars/userucid/
        var calDavClient = new Client("http://localhost:82/dav.php/calendars/testUser", "testUser", "Azerty123");

        // Get all calendars for the user.
        var calendars = await calDavClient.GetAllCalendars();

        // Get the calendar by the uid.
        var calendarByUid = await calDavClient.GetCalendarByUid("default");

        // Get the default calendar.
        var defaultCalendar = await calDavClient.GetDefaultCalendar();

        // Add an event.
        var calendarEvent = new CalendarEvent()
        {
            Description = "TestDescription1",
            Summary = "TestSummary",
            Location = "TestLocation",
            DtStart = new Ical.Net.DataTypes.CalDateTime(DateTime.Now, "UTC"),
            DtEnd = new Ical.Net.DataTypes.CalDateTime(DateTime.Now.AddHours(2), "UTC"),
        };
        var added = await calDavClient.AddOrUpdateEvent(calendarEvent, "default");

        calendarEvent.Summary = "UpdatedSummary";
        var updated = await calDavClient.AddOrUpdateEvent(calendarEvent, "default");

        // Delete an event.
        var deleted = await calDavClient.DeleteEvent(calendarEvent, "default");

    }
}
