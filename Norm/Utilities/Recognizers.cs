using HandyHangoutStudios.Parsers;
using HandyHangoutStudios.Parsers.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Norm.Utilities
{
    public static class Recognizers
    {
        public static IEnumerable<DateTimeV2ModelResult> RecognizeDateTime(string content, DateTime? refTime, params DateTimeV2Type[] types)
        {
            return DateTimeRecognizer
                .RecognizeDateTime(content, culture: Culture.English, refTime: refTime)
                .Select(model => model.ToDateTimeV2ModelResult())
                .Where(result => types.Contains(result.TypeName));
        }

        public static IEnumerable<DateTimeV2ModelResult> RecognizeDateTime(string content, params DateTimeV2Type[] types)
        {
            return RecognizeDateTime(content, null, types);
        }
    }
}
