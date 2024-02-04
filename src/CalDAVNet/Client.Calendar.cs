// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.vCalendar.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class for calendar data.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

using ICal.net.Components;
using iCalNET;
using MoreLinq;
using MoreLinq.Extensions;
using System;

/// <summary>
/// The client class for calendar data.
/// </summary>
public partial class Client
{
    /// <summary>
    /// The date time format.
    /// </summary>
    private const string DateTimeFormat = "yyyyMMddTHHmmssZ";

    /// <summary>
    /// Gets all calendars available for the current user (or none if unauthenticated).
    /// </summary>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="vCalendar"/>s.</returns>
    public async Task<IEnumerable<Calendar>> GetAllCalendars()
    {
        // Create the request body.
        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.ResourceType));
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.DisplayName));
        prop.Add(new XElement(Namespaces.ServerNs + ElementNames.GetCTag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.SupportedCalendarComponentSet));

        var root = new XElement(
            Namespaces.DavNs + ElementNames.PropFind,
            new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.Cs, Namespaces.ServerNs));
        root.Add(prop);

        // Query for data.
        var result = await this.client
            .Propfind("", root)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return new List<Calendar>();
        }

        // Get all calendars by uri.
        var calendars = new List<Calendar>();

        foreach (var resource in result.Resources)
        {
            if (!resource.Properties.Any(t => t.Value.Contains("calendar")))
            {
                continue;
            }

            var calendar = await this.GetCalendarWithUri(resource.Uri);

            if (calendar is null)
            {
                continue;
            }

            calendars.Add(calendar);
        }

        return calendars;
    }

    /// <summary>
    /// Gets a calendar by its uid.
    /// </summary>
    /// <param name="uid">The uid of the calendar.</param>
    /// <returns>The <see cref="vCalendar"/> or <c>null</c> if none was found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the uid is empty.</exception>
    public Task<Calendar?> GetCalendarByUid(string uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            throw new ArgumentNullException(nameof(uid));
        }

        return this.GetCalendarWithUri(uid);
    }

    /// <summary>
    /// Gets the default calendar for the given user.
    /// </summary>
    /// <returns>The <see cref="vCalendar"/> or <c>null</c> if none was found.</returns>
    public async Task<Calendar?> GetDefaultCalendar()
    {
        // Create the request body.
        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.ResourceType));
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.DisplayName));
        prop.Add(new XElement(Namespaces.ServerNs + ElementNames.GetCTag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.SupportedCalendarComponentSet));

        var root = new XElement(
            Namespaces.DavNs + ElementNames.PropFind,
            new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs),
            new XAttribute(XNamespace.Xmlns + ElementNames.Cs, Namespaces.ServerNs));
        root.Add(prop);

        // Query for data.
        var result = await this.client
            .Propfind("", root)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        // Get resource.
        foreach (var resource in result.Resources)
        {
            if (!resource.Properties.Any(t => t.Value.Contains("calendar")))
            {
                continue;
            }

            var calendar = await this.GetCalendarWithUri(resource.Uri);

            if (calendar is null)
            {
                continue;
            }

            return calendar;
        }

        return null;
    }

    /// <summary>
    /// Gets a calendar by its uri.
    /// </summary>
    /// <param name="uri">The uri of the calendar.</param>
    /// <returns>The <see cref="vCalendar"/> or <c>null</c> if none was found.</returns>
    private async Task<Calendar?> GetCalendarWithUri(string uri)
    {
        // Create the request body.
        var propfind = new XElement(Namespaces.DavNs + ElementNames.PropFind, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs));
        propfind.Add(new XElement(Namespaces.DavNs + ElementNames.AllProps));

        // Query for data.
        var result = await this.client
            .Propfind(uri, propfind)
            .Send()
            .ConfigureAwait(false);

        if (!result.IsSuccessful)
        {
            return null;
        }

        return await this.DeserializeCalendarResource(result.Resources, uri);
    }

    /// <summary>
    /// Deserializes the calendar resource.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="uri">The uri.</param>
    /// <param name="client">The CalDAV client.</param>
    /// <returns>The <see cref="vCalendar"/> or <c>null</c> if none was found.</returns>
    private async Task<Calendar?> DeserializeCalendarResource(IReadOnlyCollection<Resource> resources, string uri)
    {
        // Get resource.
        // Fetch the events.
        var calendarResources = resources.Where(t =>
            t.Properties.Any(x =>
                x.Key.LocalName.Equals(ElementNames.ResourceType, StringComparison.CurrentCultureIgnoreCase) &&
                x.Value.Contains("calendar", StringComparison.CurrentCultureIgnoreCase)
                ));

        var resource = calendarResources.FirstOrDefault();
        if (resource is null)
        {
            return null;
        }

        if (uri.Last() == '/')
        {
            uri = uri.Remove(uri.Length - 1, 1);
        }

        Calendar? calendar = this.GetCalendar(resource, uri);

        var events = await this.GetEvents(uri).ConfigureAwait(false);

        calendar?.Components.AddRange(events.ToList());
        return calendar;
    }

    /// <summary>
    /// Gets the events.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>A <see cref="IEnumerable{T}"/> of <see cref="CalendarEvent"/>s.</returns>
    private async Task<IEnumerable<CalendarEvent>> GetEvents(string uri)
    {
        // Create the request body.
        var query = new XElement(Namespaces.CalNs + ElementNames.CalendarQuery, new XAttribute(XNamespace.Xmlns + ElementNames.D, Namespaces.DavNs), new XAttribute(XNamespace.Xmlns + ElementNames.C, Namespaces.CalNs));

        var prop = new XElement(Namespaces.DavNs + ElementNames.Prop);
        prop.Add(new XElement(Namespaces.DavNs + ElementNames.GetETag));
        prop.Add(new XElement(Namespaces.CalNs + ElementNames.CalendarData));
        query.Add(prop);

        var filter = new XElement(Namespaces.CalNs + ElementNames.Filter);
        filter.Add(new XElement(Namespaces.CalNs + ElementNames.CompFilter, new XAttribute(ElementNames.Name, ElementNames.VCalendar)));
        query.Add(filter);

        // Query for data.
        var result = await this.client
            .Report(uri, query)
            .Send()
            .ConfigureAwait(false);

        // Parse the events.
        return await this.GetEvents(result.Resources);
    }

    private async Task<IEnumerable<CalendarEvent>> GetEvents(IEnumerable<Resource> resources)
    {
        List<CalendarEvent> events = [];
        foreach (var resource in resources.Take(1))
        {
            events.AddRange(await this.GetEvent(resource));
        }

        return events;
    }

    private async Task<IEnumerable<CalendarEvent>> GetEvent(Resource resource)
    {
        if (resource.Uri.EndsWith('/'))
        {
            resource.Uri = resource.Uri.Remove(resource.Uri.Length - 1, 1);
        }

        var calendar = resource.Properties
            .Where(x => x.Key.LocalName.Equals(ElementNames.CalendarData, StringComparison.CurrentCultureIgnoreCase))
            .Select(x => Calendar.LoadCalendarAsync(x.Value))
            .FirstOrDefault();

        return calendar != null ? (await calendar)?.GetEvents() ?? [] : [];
    }

    private Calendar? GetCalendar(Resource resource, string uri)
    {

        var calendar = Calendar.LoadCalendar(string.Empty);
        if (calendar == null)
        {
            return null;
        }

        calendar.Url = uri;

        //calendar-order
        foreach (var property in resource.Properties)
        {
            switch (property.Key.LocalName)
            {
                case ElementNames.DisplayName:
                    calendar.Name = property.Value;
                    break;

                case ElementNames.Owner:
                    calendar.Owner = property.Value.Contains(ElementNames.Href) ? MyRegex().Replace(property.Value, string.Empty) : property.Value;
                    break;

                case ElementNames.GetETag:
                    calendar.ETag = property.Value;
                    break;

                case ElementNames.GetLastModified:
                    calendar.LastModified = DateTimeOffset.Parse(property.Value, CultureInfo.InvariantCulture);
                    break;

                case ElementNames.SyncToken:
                    calendar.SyncToken = property.Value;
                    break;

                case ElementNames.CalendarColor:
                    calendar.Color = property.Value;
                    break;

                case ElementNames.CalendarDescription:
                    calendar.Description = property.Value;
                    break;

                case ElementNames.CreationDate:
                    calendar.Created = DateTimeOffset.ParseExact(property.Value, DateTimeFormat, CultureInfo.InvariantCulture);
                    break;

                case ElementNames.TimeZone:
                    //TODO
                    //calendar.TimeZone = property.Value
                    break;

                default:
                    break;
            }
        }

        calendar.Uid = uri.Split('/').LastOrDefault() ?? uri;

        return calendar;
    }

    [GeneratedRegex(".*(\\d{3}).*")]
    public static partial Regex MyRegex();
}