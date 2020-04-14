using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Ink;
using ProtoBuf;
using SteamCLI.Core;
using SteamKit2;
using static SteamKit2.SteamClient;
using static SteamKit2.SteamUser;
using static SteamKit2.SteamApps;
using static SteamKit2.SteamApps.PICSProductInfoCallback;

namespace TorchWizard.Steam
{
    public class SteamDownloader
    {
        private const string PERSISTENT_CACHE_DIR = ".sdcache";
        private const string LOCAL_CACHE_FILE = PERSISTENT_CACHE_DIR + "\\local";
        
        private readonly SteamClient  _client;    // Core Steam client.
        private readonly SteamUser    _user;      // User authentication handler.
        private readonly SteamApps    _apps;      // Steam app data handler.
        private readonly CallbackPump _callbacks; // Callback message pump.
        private readonly CdnPool      _cdnPool;   // CDN agent pool.
        
        private LoggedOnCallback _loginDetails = null; // Info about the currently logged in user.
        
        // Caches
        private readonly Dictionary<uint, byte[]> _depotKeys = new Dictionary<uint, byte[]>();
        private readonly Dictionary<string, CDNAuthTokenCallback> _cdnAuthTokens = new Dictionary<string, CDNAuthTokenCallback>();
        private readonly Dictionary<uint, PICSProductInfo> _appInfos = new Dictionary<uint, PICSProductInfo>();

        public bool IsLoggedIn => _loginDetails != null;
        public CdnPool CdnPool => _cdnPool;

        public async Task<byte[]> GetDepotKeyAsync(uint appId, uint depotId)
        {
            if (_depotKeys.TryGetValue(depotId, out var depotKey))
                return depotKey;

            var depotKeyResult = await _apps.GetDepotDecryptionKey(depotId, appId).ToTask().ConfigureAwait(false);
            _depotKeys[depotId] = depotKeyResult.DepotKey;
            return depotKeyResult.DepotKey;
        }

        public async Task<string> GetCdnAuthTokenAsync(uint appId, uint depotId, string host)
        {
            var key = $"{depotId}:{host}";

            if (_cdnAuthTokens.TryGetValue(key, out var token) && token.Expiration < DateTime.Now)
                return token.Token;

            var cdnAuthTokenResult = await _apps.GetCDNAuthToken(appId, depotId, host).ToTask().ConfigureAwait(false);
            _cdnAuthTokens[key] = cdnAuthTokenResult;
            return cdnAuthTokenResult.Token;
        }

        public async Task<PICSProductInfo> GetAppInfoAsync(uint appId)
        {
            if (_appInfos.TryGetValue(appId, out var appInfo))
                return appInfo;

            var productResult = await _apps.PICSGetProductInfo(appId, null, false).ToTask().ConfigureAwait(false);
            _appInfos[appId] = productResult.Results[0].Apps[appId];
            return _appInfos[appId];
        }
        
        public SteamDownloader(SteamConfiguration configuration)
        {
            _client = new SteamClient(configuration);
            _user = _client.GetHandler<SteamUser>();
            _apps = _client.GetHandler<SteamApps>();
            _cdnPool = new CdnPool(_client, _apps);
            
            _callbacks = new CallbackPump(_client);
            _callbacks.CallbackReceived += CallbacksOnCallbackReceived;
        }

        private void CallbacksOnCallbackReceived(ICallbackMsg obj)
        {
            Console.WriteLine(obj.GetType());
            switch (obj)
            {
                case DisconnectedCallback discon:
                    if (!discon.UserInitiated)
                        LogoutAsync();
                    break;
            }
        }

        public async Task<DepotManifest> GetManifestAsync(uint appId, uint depotId, string branch)
        {
            var appInfo = await GetAppInfoAsync(appId).ConfigureAwait(false);
            var manifestId = appInfo.GetManifestId(depotId, branch);
            return await GetManifestAsync(appId, depotId, manifestId).ConfigureAwait(false);
        }
        
        public async Task<DepotManifest> GetManifestAsync(uint appId, uint depotId, ulong manifestId)
        {
            if (!IsLoggedIn)
                throw new InvalidOperationException("The Steam client is not logged in.");
            
            var depotKey = await GetDepotKeyAsync(appId, depotId).ConfigureAwait(false);
            var cdnClient = await _cdnPool.GetClientForDepot(appId, depotId, depotKey).ConfigureAwait(false);
            var server = _cdnPool.GetBestServer();
            var cdnAuthToken = await GetCdnAuthTokenAsync(appId, depotId, server.Host).ConfigureAwait(false);
            var manifest = await cdnClient.DownloadManifestAsync(depotId, manifestId, server, cdnAuthToken, depotKey).ConfigureAwait(false);

            return manifest;
        }
        
        #region Auth
        
        /// <summary>
        /// Connect to Steam and log in with the given details, or anonymously if none are provided.
        /// </summary>
        /// <param name="details">User credentials.</param>
        /// <returns>Login details.</returns>
        /// <exception cref="Exception"></exception>
        public async Task<LoggedOnCallback> LoginAsync(LogOnDetails details = default)
        {
            if (_loginDetails != null)
                throw new InvalidOperationException("Already logged in.");
            
            _callbacks.Start();
            _client.Connect();

            var connectResult = await _callbacks
                                    .WaitForAsync(x => x is ConnectedCallback || x is DisconnectedCallback)
                                    .ConfigureAwait(false);
            
            if (connectResult is DisconnectedCallback)
                throw new Exception("Failed to connect to Steam.");
            
            if (details == null)
                _user.LogOnAnonymous();
            else
                _user.LogOn(details);

            var loginResult = await _callbacks.WaitForAsync<LoggedOnCallback>().ConfigureAwait(false);
            if (loginResult.Result != EResult.OK)
                throw new Exception($"Failed to log into Steam: {loginResult.Result:G}");
            
            await _cdnPool.Initialize((int)loginResult.CellID);
            _loginDetails = loginResult;
            return loginResult;
        }

        /// <summary>
        /// Log out the client and disconnect from Steam.
        /// </summary>
        public async Task LogoutAsync()
        {
            if (_loginDetails == null)
                return;
            
            _user.LogOff();
            _client.Disconnect();

            await _callbacks.WaitForAsync<DisconnectedCallback>().ConfigureAwait(false);
            _callbacks.Stop();
            _loginDetails = null;
            
            _appInfos.Clear();
            _depotKeys.Clear();
            _cdnAuthTokens.Clear();
        }
        
        #endregion
        
        public async Task InstallAsync(uint appId, uint depotId, string branch, string installPath)
        {
            LocalFileCache localCache;
            var localCacheFile = Path.Combine(installPath, LOCAL_CACHE_FILE);

            // Only allow installing to an existing install or empty/new directory.
            if (File.Exists(localCacheFile))
            {
                using (var fs = File.OpenRead(localCacheFile))
                    localCache = Serializer.Deserialize<LocalFileCache>(fs);
            }
            else if (Directory.Exists(installPath) && Directory.EnumerateFileSystemEntries(installPath).Any())
            {
                throw new InvalidOperationException("The target directory is not empty.");
            }
            else
            {
                localCache = new LocalFileCache();
            }

            Directory.CreateDirectory(installPath);
            
            // Get installation details from Steam
            var manifest = await GetManifestAsync(appId, depotId, branch);

            var job = InstallJob.Upgrade(appId, depotId, installPath, localCache, manifest);
            await job.Execute(this);
        }
    }
}