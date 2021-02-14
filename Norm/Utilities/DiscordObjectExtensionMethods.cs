using DSharpPlus.Entities;
using MediatR;
using NodaTime;
using NodaTime.TimeZones;
using Norm.Database.Entities;
using Norm.Database.Requests;
using System.Threading.Tasks;

namespace Norm.Utilities
{
    public static class DiscordObjectExtensionMethods
    {
        /// <summary>
        /// Verifies that this DiscordUser has a timezone registered in our database and throws a PreExecutionException if they don't
        /// </summary>
        /// <param name="user">The user who should have a timezone</param>
        /// <param name="provider">The database access provider</param>
        /// <param name="timeZoneProvider"></param>
        /// <returns></returns>
        public static async Task<(bool, DateTimeZone)> TryGetDateTimeZoneAsync(this DiscordUser user, IMediator mediator, IDateTimeZoneProvider timeZoneProvider)
        {
            UserTimeZone userTimeZone = (await mediator.Send(new UserTimeZones.GetUsersTimeZone(user))).Value;
            try
            {
                if (userTimeZone != null)
                {
                    return (true, timeZoneProvider[userTimeZone.TimeZoneId]);
                }
            }
            catch (DateTimeZoneNotFoundException)
            {
            }
            return (false, default);
        }
    }
}
