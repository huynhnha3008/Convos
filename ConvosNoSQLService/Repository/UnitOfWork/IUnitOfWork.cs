using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using Repository.GenericRepository;

namespace Repository.UnitOfWork
{
    public interface IUnitOfWork
    {
        IGenericRepository<Message> Messages { get; }
        IGenericRepository<Emoji> Emojis { get; }
        IGenericRepository<Reaction> Reactions { get; }
        IGenericRepository<Attachment> Attachments { get; }
        IGenericRepository<PrivateMessage> PrivateMessages { get; }
        IGenericRepository<PrivateCallSession> PrivateCallSessions { get; }
        IGenericRepository<Document> Documents { get; }
        IGenericRepository<Whiteboard> Whiteboards { get; }
        Task SaveAsync();
    }
}