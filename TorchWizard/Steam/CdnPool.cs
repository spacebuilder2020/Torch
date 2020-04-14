﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
 using System.Linq;
 using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;

namespace TorchWizard.Steam
{
    public class CdnPool
    {
        private readonly SteamClient _client;
        private readonly SteamApps   _apps;
        private int _cellId;
        private readonly ConcurrentBag<CDNClient> _clientBag = new ConcurrentBag<CDNClient>();
        private readonly ConditionalWeakTable<CDNClient, ClientInfo> _clientInfoTable = new ConditionalWeakTable<CDNClient, ClientInfo>();
        private IList<CDNClient.Server> _servers;
        
        private readonly CancellationTokenSource _cancelTokenSource = new CancellationTokenSource();
        
        public CdnPool(SteamClient client, SteamApps apps)
        {
            _client = client;
            _apps   = apps;
        }

        /// <summary>
        /// Initializes stuff needed to download content from the Steam content servers.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize(int cellId)
        {
            _cellId = cellId;
            _servers = (await ContentServerDirectoryService.LoadAsync(_client.Configuration, _cellId, _cancelTokenSource.Token)
                                                          .ConfigureAwait(false)).ToList();
            CDNClient.RequestTimeout = TimeSpan.FromSeconds(10);
            ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, 100);
            Console.WriteLine($"Got {_servers.Count} CDN servers.");
        }
        
        /// <summary>
        /// Connects a CDNClient to the requested depot using the given authentication information or pulls one from the pool if it already exists.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="depotId"></param>
        /// <param name="depotKey"></param>
        /// <param name="appTicket"></param>
        /// <returns></returns>
        /// <exception cref="SteamException"></exception>
        public async Task<CDNClient> GetClientForDepot(uint appId, uint depotId, byte[] depotKey)
        {
            if (_servers == null)
                return null;

            ClientInfo info;
            if (!_clientBag.TryTake(out var client))
            {
                client = new CDNClient(_client);
            }
            return client;
        }

        public void ReturnClient(CDNClient client)
        {
            _clientBag.Add(client);
        }

        private int cur = 0;
        public CDNClient.Server GetBestServer()
        {
            cur++;
            return _servers[cur % _servers.Count];
        }

        private class ClientInfo
        {
            public CDNClient.Server Server;
            public DateTime AuthExpiration = DateTime.MaxValue;
            public string AuthToken;
        }
    }
}