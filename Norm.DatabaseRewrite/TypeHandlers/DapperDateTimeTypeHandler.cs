using Dapper;
using System;
using System.Data;

namespace Norm.DatabaseRewrite.TypeHandlers
{
    // Needed because NodaTime Plugin overrides the default mappings and Dapper uses non-generic IDataReader.GetValue functions which return Instant instead of DateTime.
    public class DapperDateTimeTypeHandler : SqlMapper.TypeHandler<DateTime>
    {
        public override DateTime Parse(object value)
        {
            return value switch
            {
                DateTime dateTime => dateTime,
                NodaTime.Instant i => i.ToDateTimeUtc(),
                _ => throw new ArgumentException(
                    $"Invalid value of type '{value.GetType().FullName}' given. DateTime or NodaTime.Instant values are supported.",
                    nameof(value))
            };
        }

#pragma warning disable CA1061 // Do not hide base class methods
#pragma warning disable CA1822 // Mark members as static
        public void SetValue(IDbDataParameter parameter, object value)
        {
            parameter.Value = value;
        }

        public override void SetValue(IDbDataParameter parameter, DateTime value)
        {
            parameter.Value = value;
        }
    }
}
