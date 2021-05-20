using MediatR;

namespace Norm.Database.Requests.BaseClasses
{
    public abstract class DbRequest : IRequest<DbResult>
    {
        protected DbRequest() { }
    }

    public abstract class DbRequest<T> : IRequest<DbResult<T>>
    {
        protected DbRequest() { }
    }
}
