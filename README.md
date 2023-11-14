CalDAVNet
====================================

CalDAVNet is a project to access CalDAV servers and a fork of https://github.com/markatk/CalDAV.NET.

[![Build status](https://ci.appveyor.com/api/projects/status/0rsrutr4in3chb3k?svg=true)](https://ci.appveyor.com/project/SeppPenner/caldavnet)
[![GitHub issues](https://img.shields.io/github/issues/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/issues)
[![GitHub forks](https://img.shields.io/github/forks/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/network)
[![GitHub stars](https://img.shields.io/github/stars/SeppPenner/CalDAVNet.svg)](https://github.com/SeppPenner/CalDAVNet/stargazers)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://raw.githubusercontent.com/SeppPenner/CalDAVNet/master/License.txt)
[![Nuget](https://img.shields.io/badge/CalDAVNet-Nuget-brightgreen.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HaemmerElectronics.SeppPenner.CalDAVNet.svg)](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/)
[![Known Vulnerabilities](https://snyk.io/test/github/SeppPenner/CalDAVNet/badge.svg)](https://snyk.io/test/github/SeppPenner/CalDAVNet)
[![Gitter](https://badges.gitter.im/CalDAVNet/community.svg)](https://gitter.im/CalDAVNet/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

## Available for
* Net 7.0

## Net Core and Net Framework latest and LTS versions
* https://dotnet.microsoft.com/download/dotnet

## Basic usage
```csharp
using Ical.Net.CalendarComponents;

/// <summary>
/// The main method.
/// </summary>
public static async Task Main()
{
    // Create client.
    //Only works with basic auth
    //Test with baikal => http(s)://server_url:server_port/dav.php/calendars/userucid/
    var calDavClient = new Client("http://localhost:82/dav.php/calendars/user", "user", "pass");
    
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
```

## NuGet
The project can be found on [nuget](https://www.nuget.org/packages/HaemmerElectronics.SeppPenner.CalDAVNet/).

## Install

```bash
dotnet add package HaemmerElectronics.SeppPenner.CalDAVNet
```

Change history
--------------

See the [Changelog](https://github.com/SeppPenner/CalDAVNet/blob/master/Changelog.md).
