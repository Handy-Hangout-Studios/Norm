using DSharpPlus.CommandsNext;
using Norm.Modules;
using Norm.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Features
{
    public class NovelUpdatesFeature : IFeature
    {
        private readonly BotService bot;

        public NovelUpdatesFeature(BotService bot)
        {
            this.bot = bot;
        }

        public void Initialize()
        {
            foreach (var commands in this.bot.CommandsDict.Values)
                commands.RegisterCommands<RoyalRoadModule>();
        }
    }
}
