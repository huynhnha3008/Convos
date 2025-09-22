using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using Repository.DatabaseSettings;
using Repository.GenericRepository;

namespace Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConvosDbContext _context;

        private IGenericRepository<Message> _messages;
        private IGenericRepository<Emoji> _emojis;
        private IGenericRepository<Reaction> _reactions ;
        private IGenericRepository<Attachment> _attachments;
        private IGenericRepository<PrivateMessage> _privatemessages;
        private IGenericRepository<PrivateCallSession> _privateCallSessions;
        private IGenericRepository<Document> _documents;
        private IGenericRepository<Whiteboard> _whiteboards;
        public UnitOfWork(ConvosDbContext context)
        {
            _context = context;
        }

        public IGenericRepository<Message> Messages => _messages ??= new GenericRepository<Message>(_context, "Messages");
        public IGenericRepository<Emoji> Emojis => _emojis ??= new GenericRepository<Emoji>(_context, "Emojis");
        public IGenericRepository<Reaction> Reactions => _reactions ??= new GenericRepository<Reaction>(_context, "Reactions");
        public IGenericRepository<Attachment> Attachments => _attachments ??= new GenericRepository<Attachment>(_context, "Attachments");
        public IGenericRepository<PrivateMessage> PrivateMessages => _privatemessages ??= new GenericRepository<PrivateMessage>(_context, "PrivateMessages");
        public IGenericRepository<PrivateCallSession> PrivateCallSessions => _privateCallSessions ??= new GenericRepository<PrivateCallSession>(_context, "PrivateCallSessions");
        public IGenericRepository<Document> Documents => _documents ??= new GenericRepository<Document>(_context, "Documents");
        public IGenericRepository<Whiteboard> Whiteboards => _whiteboards ??= new GenericRepository<Whiteboard>(_context, "Whiteboards");
        public async Task SaveAsync()
        {
            await Task.CompletedTask;
        }
    }
}