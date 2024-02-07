// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Client.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   The client class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CalDAVNet;

using ICal.net.Serialization;

/// <summary>
/// The client class.
/// </summary>
public partial class Client : IDisposable
{
    /// <summary>
    /// The CalDAV client.
    /// </summary>
    private readonly CalDavClient client;
    private readonly ICalSerializor serializor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="password">The password.</param>
    /// <param name="userName">The user name.</param>
    public Client(Uri uri, string userName, string password)
    {
        this.Uri = uri;
        this.UserName = userName;
        this.client = new CalDavClient(uri, userName, password);
        this.serializor = new ICalSerializor();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Client"/> class.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="password">The password.</param>
    /// <param name="userName">The user name.</param>
    public Client(string uri, string userName, string password) : this(new Uri(uri), userName, password)
    {
        if (uri.Last() != '/')
        {
            uri += '/';
            this.Uri = new Uri(uri);
        }
    }

    /// <summary>
    /// Gets the user name to authenticate with.
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// Gets the uri of the server to connect to.
    /// </summary>
    public Uri Uri { get; }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        this.client.Dispose();
    }
}