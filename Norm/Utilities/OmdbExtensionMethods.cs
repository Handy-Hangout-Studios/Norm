using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Norm.Omdb.Types;
using Norm.Omdb.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Utilities
{
    public static class OmdbExtensionMethods
    {
        public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this OmdbMovie movieInfo, bool largePoster = false)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithTitle(movieInfo.Title)
                .WithDescription($"{movieInfo.Plot}\n[View on IMDb](https://imdb.com/title/{movieInfo.ImdbId}/)")
                .AddField("Rated", movieInfo.Rated?.ToQueryValue() ?? "Unknown", true)
                .AddField("Runtime", movieInfo.Runtime ?? "Unknown", true)
                .AddField("Language", movieInfo.Language ?? "Unknown", true)
                .AddField("Country", movieInfo.Country ?? "Unknown", true)
                .WithFooter("Details provided courtesy of OMDb");

            if (largePoster)
            {
                builder.WithImageUrl(movieInfo.Poster);
            }
            else
            {
                builder.WithThumbnail(movieInfo.Poster);
            }

            return builder;
        }
    }
}
