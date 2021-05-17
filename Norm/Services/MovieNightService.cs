using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Services
{
    public class MovieNightService
    {
        private readonly IBotService bot;
        private readonly IMediator mediator;

        public MovieNightService(IBotService bot, IMediator mediator)
        {
            this.bot = bot;
            this.mediator = mediator;
        }

        /// <summary>
        /// Generate the embed with the randomly selected movies and add emojis to allow for voting
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public void StartVoting(ulong movieNightId)
        {

        }

        /// <summary>
        /// Determine the number of votes that each movie got and then select the highest ranked movie.
        /// If there is a tie on more than one of the movies, message the movie night creator with an
        /// embed where they will break the tie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public void CalculateVotes(ulong movieNightId)
        {

        }

        /// <summary>
        /// Make an announcement about the movie starting and then begin tracking who shows up over the
        /// next two hours to see if the people who voted showed up to the movie. If they didn't, make 
        /// them not able to vote for the next weeks movie.
        /// </summary>
        /// <param name="movieNightId">ID for the movie night in the data store</param>
        public void StartMovie(ulong movieNightId)
        {

        }
    }
}
