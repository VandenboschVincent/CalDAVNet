// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.Events.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class for event data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

/// <summary>
/// The client class for event data.
/// </summary>
public partial class Client
{
    /// <summary>
    /// The calendar serializer.
    /// </summary>
    private readonly CalendarSerializer calendarSerializer = new();

    /// <summary>
    /// Deletes an event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <returns>A value indicating whether the event was deleted or not.</returns>
    public async Task<bool> DeleteEvent(CalendarEvent calendarEvent, string calendarUcid)
    {
        string eventUrl = this.GetEventUrl(calendarEvent, calendarUcid);
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
    public async Task<bool> AddOrUpdateEvent(CalendarEvent calendarEvent, string calendarUcid, Ical.Net.Calendar? calendar = null)
    {
        calendar ??= new Ical.Net.Calendar();
        string eventUrl = this.GetEventUrl(calendarEvent, calendarUcid);
        var result = await this.client
            .Put(eventUrl, this.Serialize(calendarEvent, calendar))
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
    private string GetEventUrl(CalendarEvent calendarEvent, string calendarUcid)
    {
        if (calendarEvent.Url == null || !calendarEvent.Url.ToString().StartsWith(this.client.baseUri.ToString()))
        {
            calendarEvent.Url = new Uri(CombineUri(this.client.baseUri.ToString(), calendarUcid, $"{calendarEvent.Uid}.ics"));
        }

        if (!calendarEvent.Url.ToString().EndsWith(".ics"))
        {
            calendarEvent.Url = new Uri(CombineUri(calendarEvent.Url.ToString(), $"{calendarEvent.Uid}.ics"));
        }

        return calendarEvent.Url.ToString();
    }

    /// <summary>
    /// Serializes the event.
    /// </summary>
    /// <param name="calendarEvent">The calendar event.</param>
    /// <param name="calendar">The calendar.</param>
    /// <returns>The serialized event as <see cref="string"/>.</returns>
    private string Serialize(CalendarEvent calendarEvent, Ical.Net.Calendar calendar)
    {
        calendar.Events.Clear();
        calendar.Events.Add(calendarEvent);
        return this.calendarSerializer.SerializeToString(calendar);
    }
}