using NodaTime;
using NodaTime.TimeZones;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Norm.Services
{
    public class NodaTimeConverterService
    {
        public IDateTimeZoneProvider TimeZoneProvider { get; set; }
        public IDateTimeZoneSource TimeZoneSource { get; set; }

        public NodaTimeConverterService(IDateTimeZoneProvider timeZoneProvider, IDateTimeZoneSource timeZoneSource)
        {
            this.TimeZoneProvider = timeZoneProvider;
            this.TimeZoneSource = timeZoneSource;
        }

        public TimeZoneInfo ConvertToTimeZoneInfo(DateTimeZone dateTimeZone)
        {
            TzdbDateTimeZoneSource source;
            if (dateTimeZone is BclDateTimeZone bclDateTimeZone)
            {
                return bclDateTimeZone.OriginalZone;
            }
            else if (this.TimeZoneSource is TzdbDateTimeZoneSource tzdbDateTimeZoneSource)
            {
                source = tzdbDateTimeZoneSource;
            }
            else
            {
                source = TzdbDateTimeZoneSource.Default;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return TimeZoneInfo.FindSystemTimeZoneById(source.TzdbToWindowsIds[dateTimeZone.Id]);
            }
            else
            {
                return TimeZoneInfo.FindSystemTimeZoneById(dateTimeZone.Id);
            }
        }

        public TimeZoneInfo ConvertToTimeZoneInfo(string dateTimeZoneId)
        {
            return this.ConvertToTimeZoneInfo(this.TimeZoneProvider[dateTimeZoneId]);
        }
    }
}
