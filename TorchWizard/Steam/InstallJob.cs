using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2;

namespace TorchWizard.Steam
{
    public class InstallJob
    {
        private readonly Dictionary<string, FileParts> _fileParts = new Dictionary<string,FileParts>();
        private readonly ConcurrentStack<ChunkWorkItem> _neededChunks = new ConcurrentStack<ChunkWorkItem>();
        private readonly List<string> _filesToDelete = new List<string>();
        private uint _appId;
        private uint _depotId;

        private SteamDownloader _downloader;

        public async Task Execute(SteamDownloader downloader, int workerCount = 8)
        {
            _downloader = downloader;

            var workers = new Task[workerCount];
            for (var i = 0; i < workerCount; i++)
            {
                workers[i] = StartWorkerAsync();
            }

            await Task.WhenAll(workers);
        }

        private async Task StartWorkerAsync()
        {
            while (_neededChunks.Count > 0)
            {
                if (_neededChunks.TryPop(out var workItem))
                {
                    var depotKey = await _downloader.GetDepotKeyAsync(_appId, _depotId).ConfigureAwait(false);
                    var server = _downloader.CdnPool.GetBestServer();
                    var client = await _downloader.CdnPool.GetClientForDepot(_appId, _depotId, depotKey).ConfigureAwait(false);
                    client.AuthenticateDepot(_depotId, depotKey, await _downloader.GetCdnAuthTokenAsync(_appId, _depotId, server.Host));

                    CDNClient.DepotChunk chunk;

                    try
                    {
                        chunk = await client.DownloadDepotChunkAsync(_depotId, workItem.ChunkData, server);

                        if (depotKey != null || CryptoHelper.AdlerHash(chunk.Data).SequenceEqual(chunk.ChunkInfo.Checksum))
                            await _fileParts[workItem.FileName].SubmitAsync(chunk).ConfigureAwait(false);
                        else
                            _neededChunks.Push(workItem);
                    }
                    catch
                    {
                        _neededChunks.Push(workItem);
                    }
                    
                    _downloader.CdnPool.ReturnClient(client);
                }
            }
        }
        
        public static InstallJob Upgrade(uint appId, uint depotId, string installPath, LocalFileCache localFiles, DepotManifest remoteFiles)
        {
            var job = new InstallJob();
            job._appId = appId;
            job._depotId = depotId;
            var remoteFileDict = remoteFiles.Files
                                            .Where(x => (x.Flags & EDepotFileFlag.Directory) == 0)
                                            .ToDictionary(x => x.FileName);
            
            // Find difference between local files and remote files.
            foreach (var localFile in localFiles.Files)
            {
                if (remoteFileDict.TryGetValue(localFile.Key, out var remoteFile))
                {
                    // Don't download unchanged files.
                    if (localFile.Value.Hash.SequenceEqual(remoteFile.FileHash))
                        remoteFileDict.Remove(localFile.Key);
                }
                else
                {
                    // Delete files that are present locally but not in the new manifest.
                    job._filesToDelete.Add(localFile.Key);
                }
            }

            // Populate needed chunks
            foreach (var file in remoteFileDict.Values)
            {
                job._fileParts.Add(file.FileName, new FileParts(file, installPath));
                foreach (var chunk in file.Chunks)
                {
                    job._neededChunks.Push(new ChunkWorkItem
                    {
                        ChunkData = chunk,
                        FileName = file.FileName
                    });
                }
            }
            
            return job;
        }

        private class FileParts
        {
            private readonly System.IO.FileInfo _destPath;
            private readonly DepotManifest.FileData _fileData;
            private readonly ConcurrentBag<CDNClient.DepotChunk> _completeChunks;
            
            public bool IsComplete { get; private set; }

            public FileParts(DepotManifest.FileData fileData, string basePath)
            {
                _fileData = fileData;
                _destPath = new System.IO.FileInfo(Path.Combine(basePath, fileData.FileName));
                _completeChunks = new ConcurrentBag<CDNClient.DepotChunk>();

                IsComplete = false;
            }
            
            public async Task SubmitAsync(CDNClient.DepotChunk chunk)
            {
                if (IsComplete)
                    throw new InvalidOperationException("The file is already complete.");
                
                _completeChunks.Add(chunk);
                //Console.WriteLine($"{chunk.ChunkInfo.Offset} {_destPath.FullName}");

                if (_completeChunks.Count == _fileData.Chunks.Count)
                {
                    IsComplete = true;
                    await Save().ConfigureAwait(false);
                }
            }
            
            /// <summary>
            /// Creates a physical file and writes the chunks to disk.
            /// </summary>
            /// <exception cref="InvalidOperationException">The file is not ready to be saved.</exception>
            private async Task Save()
            {
                Directory.CreateDirectory(_destPath.DirectoryName);
                
                using (var fs = File.Create(_destPath.FullName, (int)_fileData.TotalSize, 
                    FileOptions.RandomAccess | FileOptions.Asynchronous))
                {
                    fs.SetLength((long)_fileData.TotalSize);
                    while (_completeChunks.Count > 0)
                    {
                        if (_completeChunks.TryTake(out var chunk))
                        {
                            fs.Seek((long)chunk.ChunkInfo.Offset, SeekOrigin.Begin);
                            await fs.WriteAsync(chunk.Data, 0, chunk.Data.Length).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
        
        private struct ChunkWorkItem
        {
            public string FileName;
            public DepotManifest.ChunkData ChunkData;
        }
    }
}