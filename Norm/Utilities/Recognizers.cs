using HandyHangoutStudios.Parsers;
using HandyHangoutStudios.Parsers.Models;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Norm.Utilities
{
    public static class Recognizers
    {
        public static IEnumerable<DateTimeV2ModelResult> RecognizeDateTime(string content, params DateTimeV2Type[] types)
        {
            return DateTimeRecognizer
                .RecognizeDateTime(content, culture: Culture.English)
                .Select(model => model.ToDateTimeV2ModelResult())
                .Where(result => types.Contains(result.TypeName));
        }
    }
}
