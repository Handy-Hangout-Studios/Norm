using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace Norm.Services
{
    public sealed class LatexRenderService : IDisposable
    {
        private bool isDisposed = false;
        private HttpClient quickLatexHttpClient;
        private const string defaultLatexHeader =
@"\documentclass[border=10pt]{standalone}
\usepackage{amsmath}
\usepackage{amsfonts}
\usepackage{amssymb}
\usepackage{nopageno}

\begin{document}
";
        private const string defaultLatexFooter =
@"
\end{document}";

        public LatexRenderService()
        {
            quickLatexHttpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://quicklatex.com"),
            };
        }

        /// <summary>
        /// Renders the LaTeX in some way form or fashion
        /// </summary>
        /// <param name="latex"></param>
        /// <param name="color"></param>
        /// <param name="fsize"></param>
        /// <returns></returns>
        public async Task<Stream> RenderLatex(string latex, DiscordColor? color = null, int fsize = 32)
        {
            if(!latex.Contains("\\begin{document}"))
            {
                latex = $"{defaultLatexHeader}{latex}{defaultLatexFooter}";
            }

            color ??= DiscordColor.Black;
            string dump = $"formula=Unneccesary&fsize={fsize}px&fcolor={color.ToString()[1..]}&mode=0&out=1&remhost=localhost&preamble={Uri.EscapeDataString(latex)}";
            HttpResponseMessage response = await quickLatexHttpClient.PostAsync(
                "/latex3.f", 
                new StringContent(dump)
            );
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to render the LaTeX");
            }


            string picture_url =
                (await response.Content.ReadAsStringAsync())
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)[1][this.quickLatexHttpClient.BaseAddress.OriginalString.Length..];
            return await quickLatexHttpClient.GetStreamAsync(picture_url);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;
            quickLatexHttpClient.Dispose();
            isDisposed = true;
        }
    }
}
