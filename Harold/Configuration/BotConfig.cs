﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harold.Configuration
{
    public class BotConfig
    {
        public static readonly string Section = "BotConfig";

        // For bot login
        public string BotToken { get; set; }

        // For logging purposes
        public ulong DevId { get; set; }
        public ulong DevGuildId { get; set; }

        // For the invite link
        public string InviteLink { get; set; }

        // Commands Next Config Info
        public bool EnableDms { get; set; }

        public bool EnableMentionPrefix { get; set; }

        public bool EnablePrefixResolver { get; set; }

        public string[] Prefixes { get; set; }

        public DatabaseConfig Database { get; set; }
    }
}
