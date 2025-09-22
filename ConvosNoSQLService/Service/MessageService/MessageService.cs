using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObject.Models;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Repository.UnitOfWork;


namespace Service.MessageService
{
    public class MessageService : IMessageService
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<MessageService> _logger;
        public MessageService(
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<MessageService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Message> GetMessageByIdAsync(string id)
        {
            return await _unitOfWork.Messages.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Message>> GetAllMessagesAsync()
        {
            return await _unitOfWork.Messages.GetAllAsync();
        }

        public async Task<Message> CreateMessageAsync(Message message)
        {
            var addedMessage = await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.SaveAsync();

            return addedMessage;
        }

        public async Task<Message> UpdateMessageAsync(string id, Message message)
        {
            var updatedMessage = await _unitOfWork.Messages.UpdateAsync(id, message);
            await _unitOfWork.SaveAsync();

            return updatedMessage;
        }

        public async Task<Message> DeleteMessageAsync(string id)
        {
            var deletedMessage = await _unitOfWork.Messages.DeleteAsync(id);
            await _unitOfWork.SaveAsync();

            return deletedMessage;
        }

        public async Task<Message> AddReactionAsync(string messageId, string memberId, string emojiIcon)
        {
            var message = await _unitOfWork.Messages.GetByIdAsync(messageId);
            if (message == null)
            {
                throw new Exception("Message not found.");
            }

            var reaction = new Reaction
            {
                MemberId = memberId,
                Emoji = emojiIcon,
                MessageId = messageId
            };

            message.Reactions.Add(reaction);

            var updatedMessage = await _unitOfWork.Messages.UpdateAsync(messageId, message);
            await _unitOfWork.SaveAsync();

            return updatedMessage;
        }
    }
}