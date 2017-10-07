using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Sof.Vlc.Http;
using Sof.Vlc.Http.Data;

namespace VISE.Managers
{
    class DirectoryManager
    {
        public DirectoryManager(VlcClient client)
        {
            Client = client;
        }

        public VlcClient Client { get; set; }

        private async Task<VlcPlaylistLeaf> GetCurrentLeaf()
        {
            var playlist = await Client.GetPlaylist();
            VlcPlaylistLeaf current = null;

            foreach (var playlistNode in playlist)
                if (playlistNode.Leaves != null && playlistNode.Leaves.Any(i => i.Current != null))
                    current = playlistNode.Leaves.First(i => i.Current != null);
            return current;
        }

        public async Task AddNextEpisode()
        {
            try
            {
                var current = await GetCurrentLeaf();

                var filepath = WebUtility.UrlDecode(current?.Uri).Replace("file:///", "");
                var path = Path.GetDirectoryName(filepath);
                var dir = (await Client.GetDirectoryContents(path)).OrderBy(i => i.Name).ToList();
                dir.Remove(dir.First());

                var next = dir.SkipWhile(i => i.Path != filepath).Take(2).ToList();

                if (next.Count == 2)
                {
                    await Client.Add(next.Last().Path);
                }

            }
            catch
            {
                // ignored
            }
        }
    }
}