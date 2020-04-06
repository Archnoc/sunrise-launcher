﻿using Qml.Net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace sunrise_launcher
{
    [Signal("update")]
    [Signal("message", NetVariantType.String)]
    [Signal("progress", NetVariantType.String, NetVariantType.String, NetVariantType.Int, NetVariantType.Int)]
    public class ServerList
    {
        private readonly ManifestFactory manifestFactory = new ManifestFactory();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private HttpClient client = new HttpClient();
        public List<Server> Servers { get; set; }
        public string Selected { get; set; }

        public ServerList()
        {
            Servers = new List<Server>();
        }

        public async Task LoadAsync(string path)
        {
            var file = ServerFile.Load(path);
            if (file == null)
                return;

            Selected = file.Selected;
            foreach (var server in file.Servers)
            {
                Servers.Add(server);
            }
            this.ActivateSignal("update");

            await RefreshAsync();
        }

        public async Task RefreshAsync()
        {
            foreach (var server in Servers)
            {
                server.State = State.Unchecked;
                server.TaskName = "Queued for Update";
            }

            foreach (var server in Servers)
            {
                await UpdateAsync(server, false);
                this.ActivateSignal("update");
                ShowMessage(server.Error);
            }
        }

        public void Save(string path)
        {
            var file = new ServerFile();
            file.Servers = Servers;
            file.Selected = Selected;
            file.Save(path);
        }

        public async Task AddAsync(string manifesturl, string installpath)
        {
            manifesturl = manifesturl.Trim();
            if (Servers.Any(x => x.ManifestURL == manifesturl))
            {
                ShowMessage("Manifest could not be added, duplicate manifest url found.");
                return;
            }

            var server = new Server();
            server.ManifestURL = manifesturl;
            server.InstallPath = installpath.Trim(); //td; check install path

            await GetInfoAsync(server);
            if (server.State == State.Error)
            {
                ShowMessage(server.Error);
                return;
            }

            Servers.Add(server);
            Selected = server.ManifestURL;
            this.ActivateSignal("update");

            await UpdateAsync(server, false);
            this.ActivateSignal("update");
            ShowMessage(server.Error);
        }

        public async Task ConfigAsync(string oldmanifesturl, string manifesturl, string installpath)
        {
            var server = Get(oldmanifesturl);
            if (server == null) return;
            server.ManifestURL = manifesturl.Trim();
            server.InstallPath = installpath.Trim(); //td; check install path

            await GetInfoAsync(server);
            this.ActivateSignal("update");

            await UpdateAsync(server, false);
            this.ActivateSignal("update");
        }

        public void Remove(string name)
        {
            var server = Get(name);
            if (server == null)
                return;
            Servers.Remove(server);
            this.ActivateSignal("update");
        }

        public Server Get(string url)
        {
            if (url == null)
                return null;

            return Servers.FirstOrDefault(x => x.ManifestURL == url);
        }

        public Server GetSelected()
        {
            if (Selected == null) return null;
            return Get(Selected);
        }

        public List<Server> GetServerInfo()
        {
            return Servers;
        }

        private void ShowMessage(string message)
        {
            if (message == null) return;
            this.ActivateSignal("message", message);
        }

        public async Task VerifyAsync()
        {
            var server = GetSelected();
            if (server == null) return;
            await UpdateAsync(server, true);
            ShowMessage(server.Error);
        }

        //this is called only when a new server is added
        private async Task GetInfoAsync(Server server)
        {
            if (server.State == State.Updating)
                server.State = State.Updating;
            server.Error = null;

            try
            {
                var manifest = await manifestFactory.Get(server);
                if (manifest == null)
                {
                    Console.WriteLine("Could not retrieve manifest from '{0}'", server.ManifestURL);
                    server.State = State.Error;
                    server.Error = "Could not retrieve manifest";
                    return;
                }

                var metadata = manifest.GetMetadata();
                if (!metadata.Verify())
                {
                    Console.WriteLine("Manifest metadata failed inspection '{0}'", server.ManifestURL);
                    server.State = State.Error;
                    server.Error = "Metadata failed inspection";
                    return;
                }

                server.Title = metadata.Title;
                server.LaunchPath = metadata.LaunchPath;
                server.LaunchEnv = metadata.LaunchEnv;
                server.LaunchArgs = metadata.LaunchArgs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GetInfoAsync: {0}", ex.Message);
                server.State = State.Error;
                server.Error = "Unknown Exception";
            }
        }

        private async Task UpdateAsync(Server server, bool force)
        {
            if (server.State == State.Updating)
                return;
            server.State = State.Updating;
            server.Error = null;

            try
            {
                UpdateProgress(server, "Retrieving Manfiest", 0, 0);
                var manifest = await manifestFactory.Get(server);
                if (manifest == null)
                {
                    Console.WriteLine("Could not retrieve manifest from '{0}'", server.ManifestURL);
                    server.State = State.Ready;
                    server.Error = "Could not retrieve manifest. You may still attempt to play, but you may be missing updates.";
                    return;
                }

                var metadata = manifest.GetMetadata();
                if (!metadata.Verify())
                {
                    Console.WriteLine("Manifest metadata failed inspection '{0}'", server.ManifestURL);
                    server.State = State.Ready;
                    server.Error = "Manifest failed inspection. You may still attempt to play, but you may be missing updates.";
                    return;
                }

                if (force || metadata.Hash != server.Hash)
                {
                    await updatefiles(server, manifest, server.InstallPath);
                }
                else
                {
                    server.State = State.Ready;
                }

                if (server.State == State.Ready)
                {
                    server.Title = metadata.Title;
                    server.Hash = metadata.Hash;
                    server.LaunchPath = metadata.LaunchPath;
                    server.LaunchEnv = metadata.LaunchEnv;
                    server.LaunchArgs = metadata.LaunchArgs;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception in UpdateAsync: {0}", ex.Message);
                server.State = State.Error;
                server.Error = "Unknown Exception";
            }
            finally
            {
                UpdateProgress(server, "", 0, 0);
            }
        }

        private async Task<bool> checkfile(ManifestFile file, Server server)
        {
            return await Task.Run(() =>
            {
                var path = Path.Combine(server.InstallPath, file.Path);
                Console.WriteLine("checking file {0}", path);

                if (!File.Exists(path))
                {
                    return file.Size == 0;
                }
                else if (file.Size == 0)
                {
                    File.Delete(path);
                    return true;
                }

                var hash = Hashing.GetHashAlgorithm(file);
                if (hash == null) return false;

                byte[] checksum;
                long size = 0;
                try
                {
                    using (FileStream filestream = new FileStream(path, FileMode.Open))
                    {
                        size = filestream.Length;
                        checksum = hash.ComputeHash(filestream);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception while verifying {0}: {1}", file.Path, ex.Message);
                    return false;
                }

                return (size == file.Size && Hashing.VerifyChecksum(checksum, file));

            });
        }

        private async Task updatefiles(Server server, IManifest manifest, string path)
        {
            await semaphore.WaitAsync();
            try
            {
                var i = 0;
                foreach (var file in manifest.GetFiles())
                {
                    UpdateProgress(server, "verifying " + file.Path, i++, manifest.Count());

                    if (!file.Verify())
                    {
                        server.State = State.Error;
                        server.Error = "Manifest file failed inspection " + file.Path;
                        return;
                    }

                    if (!await updatefile(file, server))
                    {
                        server.State = State.Error;
                        server.Error = "Could not update file " + file.Path;
                        return;
                    }
                }
            }
            finally
            {
                semaphore.Release();
                UpdateProgress(server, "", 0, 0);
            }
            server.State = State.Ready;
        }

        private async Task<bool> updatefile(ManifestFile file, Server server)
        {
            if (await checkfile(file, server))
                return true;

            var path = Path.Combine(server.InstallPath, file.Path);
            var tempfile = path + "~";
            Console.WriteLine("updating file {0}", path);
            UpdateProgress(server, "downloading " + file.Path, server.TaskDone, server.TaskCount);

            Shuffler.Shuffle(file.Sources);
            foreach (var source in file.Sources)
            {
                Console.WriteLine("downloading from source '{0}'.", source.URL);
                try
                {
                    var hash = Hashing.GetHashAlgorithm(file);
                    if (hash == null) return false;

                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    long size = 0;
                    byte[] checksum;
                    var response = await client.GetAsync(source.URL);
                    if (response.IsSuccessStatusCode)
                    {
                        using (var reader = await response.Content.ReadAsStreamAsync())
                        using (var hashstream = new CryptoStream(reader, hash, CryptoStreamMode.Read))
                        using (var filestream = new FileStream(tempfile, FileMode.Create))
                        {
                            await hashstream.CopyToAsync(filestream);
                            checksum = hash.Hash;
                            size = reader.Length;
                        }

                        if (size == file.Size && Hashing.VerifyChecksum(checksum, file))
                        {
                            File.Move(tempfile, path, true);
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("size or hash did not match manifest from source {0}", source.URL);
                            File.Delete(tempfile);
                        }
                    }
                    else
                    {
                        Console.WriteLine("cannot get file from source {0}", source.URL);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("exception while downloading source {0}: {0}", source.URL, ex.Message);
                }
            }
            return false;
        }

        public void Launch()
        {
            var server = GetSelected();
            if (server == null) return;

            if (server.State != State.Ready)
                return;

            try
            {
                var fullpath = Path.Combine(server.InstallPath, server.LaunchPath);

                var process = new Process();
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(fullpath);
                process.StartInfo.FileName = fullpath;
                process.StartInfo.Arguments = server.LaunchArgs;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                process.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: exception while launching: {0}", ex.Message);
            }
        }

        private void UpdateProgress(Server server, string name, int done, int count)
        {
            server.TaskName = name;
            server.TaskDone = done;
            server.TaskCount = count;
            this.ActivateSignal("progress", server.ManifestURL, name, done, count);
        }
    }
}