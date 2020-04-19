﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
 using System.Threading;
using System.Threading.Tasks;
using SteamKit2;
using static SteamKit2.SteamApps;

 namespace TorchSetup.Steam
{
    public class CdnPool
    {
        private readonly SteamClient _client;
        private int _cellId;
        private readonly ConcurrentBag<CDNClient> _clientBag = new ConcurrentBag<CDNClient>();
        public IList<CDNClient.Server> Servers;
        
        public CdnPool(SteamClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Initializes stuff needed to download content from the Steam content servers.
        /// </summary>
        /// <returns></returns>
        public async Task Initialize(int cellId)
        {
            _cellId = cellId;
            Servers = (await ContentServerDirectoryService.LoadAsync(_client.Configuration, _cellId, CancellationToken.None)
                                                          .ConfigureAwait(false)).ToList();
            CDNClient.RequestTimeout = TimeSpan.FromSeconds(10);
            ServicePointManager.DefaultConnectionLimit = Math.Max(ServicePointManager.DefaultConnectionLimit, 100);
            Console.WriteLine($"Got {Servers.Count} CDN servers.");
        }
        
        public CDNClient TakeClient()
        {
            if (Servers == null)
                return null;
            
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
            return Servers[cur % Servers.Count];
        }
    }
}