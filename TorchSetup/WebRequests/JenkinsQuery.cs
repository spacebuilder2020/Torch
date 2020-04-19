using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using Torch.API;

namespace Torch.WebRequests
{
    public class JenkinsQuery
    {
        private const string BRANCH_QUERY = "https://build.torchapi.net/job/Torch/job/Torch/job/{0}/" + API_PATH;
        private const string ARTIFACT_PATH = "artifact/bin/torch-server.zip";
        private const string API_PATH = "api/json";

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static JenkinsQuery _instance;
        private readonly HttpClient _client;

        private JenkinsQuery()
        {
            _client = new HttpClient();
        }

        public static JenkinsQuery Instance => _instance ?? (_instance = new JenkinsQuery());

        public async Task<Job> GetLatestVersion(string branch)
        {
            var h = await _client.GetAsync(string.Format(BRANCH_QUERY, branch));
            if (!h.IsSuccessStatusCode)
            {
                Log.Error($"Branch query failed with code {h.StatusCode}");
                if (h.StatusCode == HttpStatusCode.NotFound)
                    Log.Error("This likely means you're trying to update a branch that is not public on Jenkins. Sorry :(");
                return null;
            }

            var r = await h.Content.ReadAsStringAsync();

            BranchResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<BranchResponse>(r);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to deserialize branch response!");
                return null;
            }

            h = await _client.GetAsync($"{response.LastStableBuild.URL}{API_PATH}");
            if (!h.IsSuccessStatusCode)
            {
                Log.Error($"Job query failed with code {h.StatusCode}");
                return null;
            }

            r = await h.Content.ReadAsStringAsync();

            Job job;
            try
            {
                job = JsonConvert.DeserializeObject<Job>(r);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to deserialize job response!");
                return null;
            }

            return job;
        }

        public async Task<bool> DownloadRelease(Job job, string path)
        {
            var h = await _client.GetAsync(job.URL + ARTIFACT_PATH);
            if (!h.IsSuccessStatusCode)
            {
                Log.Error($"Job download failed with code {h.StatusCode}");
                return false;
            }

            var s = await h.Content.ReadAsStreamAsync();
            using (var fs = new FileStream(path, FileMode.Create))
            {
                await s.CopyToAsync(fs);
                await fs.FlushAsync();
            }

            return true;
        }
    }

    public class BranchResponse
    {
        public Build LastBuild;
        public Build LastStableBuild;
        public string Name;
        public string URL;
    }

    public class Build
    {
        public int Number;
        public string URL;
    }

    public class Job
    {
        private InformationalVersion _version;
        public bool Building;
        public string Description;
        public int Number;
        public string Result;
        public string URL;

        public InformationalVersion Version
        {
            get
            {
                if (_version == null)
                    InformationalVersion.TryParse(Description, out _version);

                return _version;
            }
        }
    }
}