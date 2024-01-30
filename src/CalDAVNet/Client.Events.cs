// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.Events.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class for event data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

using ICal.net.Components;
using iCalNET;

/// <summary>
/// The client class for event data.
/// </summary>
public partial class Client
{
    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>A value indicating whether the event was deleted or not.</returns>
    public async Task<bool> DeleteEvent(CalendarEvent calendarEvent, Calendar calendar)
    {
        string eventUrl = this.GetEventUrl(calendarEvent, calendar);
        var result = await this.client
            .Delete(eventUrl)
            .Send()
            .ConfigureAwait(false);

        return result.IsSuccessful;
    }

    /// <summary>
    /// AddOrUpdate an event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <param name="calendar">The calendar.</param>
    /// <returns>A value indicating whether the event was added or updated or not.</returns>
    public async Task<bool> AddOrUpdateEvent(CalendarEvent calendarEvent, Calendar calendar)
    {
        string eventUrl = this.GetEventUrl(calendarEvent, calendar);
        var result = await this.client
            .Put(eventUrl, calendarEvent.Serialize())
            .Send()
            .ConfigureAwait(false);

        return result.IsSuccessful;
    }

    private static string CombineUri(params string[] uris)
    {
        string joinedString = string.Join('/', uris);
        joinedString = joinedString.Replace("//", "/").Replace(":/", "://");
        return joinedString;
    }

    /// <summary>
    /// Gets the event url.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>The event url.</returns>
    private string GetEventUrl(CalendarEvent calendarEvent, Calendar calendar)
    {
        if (calendarEvent.Url == null || !calendarEvent.Url.ToString().StartsWith(this.client.baseUri.ToString()))
        {
            calendarEvent.Url = new Uri(CombineUri(this.client.baseUri.ToString(), calendar.Uid!, $"{calendarEvent.Uid}.ics")).ToString();
        }

        if (!calendarEvent.Url.ToString().EndsWith(".ics"))
        {
            calendarEvent.Url = new Uri(CombineUri(calendarEvent.Url.ToString(), $"{calendarEvent.Uid}.ics")).ToString();
        }

        return calendarEvent.Url.ToString();
    }
}