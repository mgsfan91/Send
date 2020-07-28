using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using RabbitMQ.Client.Framing.Impl;
using RabbitMQ.Client.Impl;



namespace Send
{
  class ConnectionFactory1
  {
    public const ushort DefaultChannelMax = 2047;
    public static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(30);
    public const uint DefaultFrameMax = 0;
    public static readonly TimeSpan DefaultHeartbeat = TimeSpan.FromSeconds(60); //
    public const string DefaultPass = "guest";
    public const string DefaultUser = "guest";
    public const string DefaultVHost = "/";
          
    public static SslProtocols DefaultAmqpUriSslProtocols { get; set; } =
        SslProtocols.None;
        public SslProtocols AmqpUriSslProtocols { get; set; } =
        DefaultAmqpUriSslProtocols;
      public static readonly IList<IAuthMechanismFactory> DefaultAuthMechanisms =
        new List<IAuthMechanismFactory>() { new PlainMechanismFactory() };
    public IList<IAuthMechanismFactory> AuthMechanisms { get; set; } = DefaultAuthMechanisms;
        public static System.Net.Sockets.AddressFamily DefaultAddressFamily { get; set; }
        public bool AutomaticRecoveryEnabled { get; set; } = true;
        public bool DispatchConsumersAsync { get; set; } = false;
        public int ConsumerDispatchConcurrency { get; set; } = 1;
        public string HostName { get; set; } = "localhost";
        public TimeSpan NetworkRecoveryInterval { get; set; } = TimeSpan.FromSeconds(5);
    private TimeSpan _handshakeContinuationTimeout = TimeSpan.FromSeconds(10);
    private TimeSpan _continuationTimeout = TimeSpan.FromSeconds(20);
        private Uri _uri;
        public TimeSpan HandshakeContinuationTimeout
    {
      get { return _handshakeContinuationTimeout; }
      set { _handshakeContinuationTimeout = value; }
    }
        public TimeSpan ContinuationTimeout
    {
      get { return _continuationTimeout; }
      set { _continuationTimeout = value; }
    }

    public Func<IEnumerable<AmqpTcpEndpoint>, IEndpointResolver> EndpointResolverFactory { get; set; } =
        endpoints => new DefaultEndpointResolver(endpoints);
    public int Port { get; set; } = AmqpTcpEndpoint.UseDefaultPort;
    public TimeSpan RequestedConnectionTimeout { get; set; } = DefaultConnectionTimeout;
    public TimeSpan SocketReadTimeout { get; set; } = DefaultConnectionTimeout;
    public TimeSpan SocketWriteTimeout { get; set; } = DefaultConnectionTimeout;
    public SslOption Ssl { get; set; } = new SslOption();
    public bool TopologyRecoveryEnabled { get; set; } = true;
  public ConnectionFactory1()
  {
   // ClientProperties = Connection.DefaultClientProperties();
      IDictionary<string, object> table = new Dictionary<string, object>
      {
        ["product"] = Encoding.UTF8.GetBytes("RabbitMQ"),
        ["version"] = Encoding.UTF8.GetBytes("BOLIMEKURAC"),
        ["platform"] = Encoding.UTF8.GetBytes(".NET"),
        ["copyright"] = Encoding.UTF8.GetBytes("Copyright (c) 2007-2020 VMware, Inc."),
        ["information"] = Encoding.UTF8.GetBytes("Licensed under the MPL. See https://www.rabbitmq.com/")
      };
      ClientProperties = table;
    }
  public AmqpTcpEndpoint Endpoint
  {
    get { return new AmqpTcpEndpoint(HostName, Port, Ssl); }
    set
    {
      Port = value.Port;
      HostName = value.HostName;
      Ssl = value.Ssl;
    }
  }
  public IDictionary<string, object> ClientProperties { get; set; }
  public string Password { get; set; } = DefaultPass;
  public ushort RequestedChannelMax { get; set; } = DefaultChannelMax;
  public uint RequestedFrameMax { get; set; } = DefaultFrameMax;
  public TimeSpan RequestedHeartbeat { get; set; } = DefaultHeartbeat;
  public string UserName { get; set; } = DefaultUser;
  public string VirtualHost { get; set; } = DefaultVHost;
   
    public Uri Uri
    {
      get { return _uri; }
      set { SetUri(value); }
    }
        
  private void SetUri(Uri uri)
  {
  Endpoint = new AmqpTcpEndpoint();

  if (string.Equals("amqp", uri.Scheme, StringComparison.OrdinalIgnoreCase))
  {
    // nothing special to do
  }
  else if (string.Equals("amqps", uri.Scheme, StringComparison.OrdinalIgnoreCase))
  {
    Ssl.Enabled = true;
    Ssl.Version = AmqpUriSslProtocols;
    Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateNameMismatch;
    Port = AmqpTcpEndpoint.DefaultAmqpSslPort;
  }
  else
  {
    throw new ArgumentException($"Wrong scheme in AMQP URI: {uri.Scheme}");
  }
  string host = uri.Host;
  if (!string.IsNullOrEmpty(host))
  {
    HostName = host;
  }
  Ssl.ServerName = HostName;

  int port = uri.Port;
  if (port != -1)
  {
    Port = port;
  }

  string userInfo = uri.UserInfo;
  if (!string.IsNullOrEmpty(userInfo))
  {
    string[] userPass = userInfo.Split(':');
    if (userPass.Length > 2)
    {
      throw new ArgumentException($"Bad user info in AMQP URI: {userInfo}");
    }
    UserName = UriDecode(userPass[0]);
    if (userPass.Length == 2)
    {
      Password = UriDecode(userPass[1]);
    }
  }
    if (uri.Segments.Length > 2)
  {
    throw new ArgumentException($"Multiple segments in path of AMQP URI: {string.Join(", ", uri.Segments)}");
  }
  if (uri.Segments.Length == 2)
  {
    VirtualHost = UriDecode(uri.Segments[1]);
  }

  _uri = uri;
  }
    private static string UriDecode(string uri)
    {
      return System.Uri.UnescapeDataString(uri.Replace("+", "%2B"));
    }
   public string ClientProvidedName { get; set; }
   public IAuthMechanismFactory AuthMechanismFactory(IList<string> mechanismNames)
   {
     // Our list is in order of preference, the server one is not.
     for (int index = 0; index < AuthMechanisms.Count; index++)
     {
       IAuthMechanismFactory factory = AuthMechanisms[index];
       string factoryName = factory.Name;
       if (mechanismNames.Any<string>(x => string.Equals(x, factoryName, StringComparison.OrdinalIgnoreCase)))
       {
         return factory;
       }
     }

     return null;
   }
 public IConnection CreateConnection()
 {
 return CreateConnection(ClientProvidedName);
 }
 public IConnection CreateConnection(string clientProvidedName)
 {
 return CreateConnection(EndpointResolverFactory(LocalEndpoints()), clientProvidedName);
 }
 public IConnection CreateConnection(IList<string> hostnames)
 {
 return CreateConnection(hostnames, ClientProvidedName);
 }
 public IConnection CreateConnection(IList<string> hostnames, string clientProvidedName)
 {
 IEnumerable<AmqpTcpEndpoint> endpoints = hostnames.Select(h => new AmqpTcpEndpoint(h, Port, Ssl));
 return CreateConnection(EndpointResolverFactory(endpoints), clientProvidedName);
 }
    private List<AmqpTcpEndpoint> LocalEndpoints()
    {
      return new List<AmqpTcpEndpoint> { Endpoint };
    }
 public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints)
 {
 return CreateConnection(endpoints, ClientProvidedName);
 }
     public IConnection CreateConnection(IList<AmqpTcpEndpoint> endpoints, string clientProvidedName)
 {
 return CreateConnection(EndpointResolverFactory(endpoints), clientProvidedName);
 }
 public IConnection CreateConnection(IEndpointResolver endpointResolver, string clientProvidedName)
 {
 IConnection conn;
 try
 {
   if (AutomaticRecoveryEnabled)
   {
     var autorecoveringConnection = new AutorecoveringConnection(this, clientProvidedName);
     autorecoveringConnection.Init(endpointResolver);
     conn = autorecoveringConnection;
   }
   else
   {
     var protocol = new RabbitMQ.Client.Framing.Protocol();
     conn = protocol.CreateConnection(this, false, endpointResolver.SelectOne(CreateFrameHandler), clientProvidedName);
   }
 }
 catch (Exception e)
 {
   throw new BrokerUnreachableException(e);
 }

 return conn;
 }
    
 internal IFrameHandler CreateFrameHandler(AmqpTcpEndpoint endpoint)
 {
 IFrameHandler fh = Protocols.DefaultProtocol.CreateFrameHandler(endpoint, SocketFactory,
     RequestedConnectionTimeout, SocketReadTimeout, SocketWriteTimeout);
 return ConfigureFrameHandler(fh);
 }

 private IFrameHandler ConfigureFrameHandler(IFrameHandler fh)
 {
 // TODO: add user-provided configurator, like in the Java client
 fh.ReadTimeout = RequestedHeartbeat;
 fh.WriteTimeout = RequestedHeartbeat;

 if (SocketReadTimeout > RequestedHeartbeat)
 {
   fh.ReadTimeout = SocketReadTimeout;
 }

 if (SocketWriteTimeout > RequestedHeartbeat)
 {
   fh.WriteTimeout = SocketWriteTimeout;
 }

 return fh;
 }


    /*


 */

  }
}
