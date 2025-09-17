using Core.Domain.BoundedContext.Requests.Entities;

namespace Core.Domain.BoundedContext.Chats.Entities;

public class Chat
{
    public Chat(Request? request)
    {
        Request = request;
    }


    public Request?  Request { get;  }
}