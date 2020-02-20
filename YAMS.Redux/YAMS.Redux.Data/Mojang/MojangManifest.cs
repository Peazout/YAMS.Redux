﻿using System;
using System.Collections.Generic;

namespace YAMS.Redux.Data.Mojang
{
    public class MojangManifest
    {
        public latest Latest { get; set; }
        public List<MojangVersion> versions { get; set; }

        /// <summary>
        /// Get the latest release from mojang
        /// </summary>
        /// <returns></returns>
        public MojangVersion GetLatestRelease()
        {
            if (Latest == null) return null;
            return versions.Find(v => v.id == Latest.release);
        }
        /// <summary>
        /// Get the latest weekly snaposhot
        /// </summary>
        /// <returns></returns>
        public MojangVersion GetLatestSnapshot()
        {
            if (Latest == null) return null;
            return versions.Find(v => v.id == Latest.snapshot);
        }

        public MojangVersion GetRelease(MinecraftServerType servertype)
        {
            switch (servertype)
            {
                case MinecraftServerType.Vanilla: return GetLatestRelease();
                case MinecraftServerType.Snapshot: return GetLatestSnapshot();
            }

            return null;

        }

    }


    public class MojangVersion
    {
        public string id { get; set; }
        public string type { get; set; }
        public DateTime time { get; set; }
        public DateTime releaseTime { get; set; }
        public string Url { get; set; }
    }

    public class latest
    {
        public string snapshot { get; set; }
        public string release { get; set; }
    }

    public class MojangDownloadFile
    {
        public MojangAsset assetIndex { get; set; }
        public string assets { get; set; }
        public MojangDownloads downloads { get; set; }
        public string id { get; set; }

    }

    public class MojangDownloads
    {
        public string id { get; set; }
        public MojangDownloadsItem client { get; set; }
        public MojangDownloadsItem server { get; set; }

    }

    public class MojangDownloadsItem
    {
        public string sha1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
    }

    public class MojangAsset
    {
        public string id { get; set; }
        public string sh1 { get; set; }
        public int size { get; set; }
        public string url { get; set; }
        public int totalSize { get; set; }

    }

}
