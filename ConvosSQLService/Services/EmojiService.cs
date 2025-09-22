using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.SignalR.Interfaces;
using Services.SignalR;
using Repositories.Interfaces;
using BusinessObjects.QueryObject;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    public class EmojiService : IEmojiService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ServerHub, IServerHub> _hubContext;
        private readonly IPermissionService _permissionService;
        private readonly FirebaseService _firebaseService;

        public EmojiService(IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService, FirebaseService firebaseService)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _permissionService = permissionService;
            _firebaseService = firebaseService;

        }

        public async Task<EmojiCreateResponse> CreateAsync(EmojiCreateRequest request, Guid userId, IFormFile imageFile)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(request.ServerId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");

            }
            var member = server.ServerMembers.SingleOrDefault(sm => sm.Id.Equals(request.ServerMemberId));
            if (member == null)
            {
                throw new InvalidDataException("Member is not existed in Server");

            }

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageEmojiPermissionAsync(server.Id, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to CREATE emoji in this server");
                    }
                }
            }
            var serverEmoList = await _unitOfWork.Emojis.GetAllServerEmoAsync(server.Id);
            if(serverEmoList != null)
            {
                // check duplicate emo Name
                foreach (var emoji in serverEmoList)
                {
                    if (emoji.Name.Equals(request.Name))
                    {
                        throw new InvalidOperationException("Creation failed: Duplicated Emoji's name");
                    }
                }
            }
            string imageUrl;
            using (var stream = imageFile.OpenReadStream())
            {
                imageUrl = await _firebaseService.UploadAvatarAsync(stream, imageFile.FileName);
            }

            Emoji emo = new Emoji
            {
                Image = imageUrl,
                ServerMemberId = request.ServerMemberId,
                Name = request.Name,
                ServerId = request.ServerId,
            };
            var createdEmo = await _unitOfWork.Emojis.CreateAsync(emo);
            await _hubContext.Clients.Group(server.Id.ToString()).CreateEmoji(server.Id, emo.Name);

            return ToEmoResponse(createdEmo);

        }

        private async Task<bool> HasManageEmojiPermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageEmojiPermission = userPermission.Any(p => p.Code.Equals(PermissionEnum.MANAGE_EMOJIS.ToString()));

            if (!hasManageEmojiPermission)
            {
                return false;
            }
            return true;
        }
        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            var emo = await _unitOfWork.Emojis.GetByIdAsync(id);
            if (emo == null)
            {
                throw new InvalidDataException("Emoji is not found");
            }
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(emo.ServerId);
            if(server  == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageEmojiPermissionAsync(emo.ServerId, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to DELETE emoji in this server");
                    }
                }
            }
            await _unitOfWork.Emojis.DeleteAsync(emo);
            await _hubContext.Clients.Group(emo.ServerId.ToString()).DeleteEmoji(emo.ServerId, emo.Name);
            return $"Emoji {emo.Name} has been deleted successfully";
        }

        public async Task<List<EmojiCreateResponse>> GetAllAsync(Guid serverId, QueryEmoji query)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var serverEmojis = await _unitOfWork.Emojis.GetAllServerEmoAsync(serverId);
            if (serverEmojis == null)
            {
                return new List<EmojiCreateResponse>();
            }
            var queryEmo = await _unitOfWork.Emojis.SearchAsync(query.SearchTerm);


            var listEmo = serverEmojis.Where(emoji => queryEmo.Any(i => i.Id == emoji.Id)).ToList();

            listEmo = query.IsDescending ? listEmo.OrderByDescending(e => e.Name).ToList() : listEmo.OrderBy(e => e.Name).ToList();

            var tmp = new List<EmojiCreateResponse>();
            foreach (var emo in listEmo)
            {
                tmp.Add(ToEmoResponse(emo));
            }
            var paginatedEmojis = tmp
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedEmojis;
        }
        private EmojiCreateResponse ToEmoResponse(Emoji emoji)
        {
            ServerCreateResponse serverRes = new ServerCreateResponse
            {
                CreatedAt = emoji.Server.CreatedAt,
                Icon = emoji.Server.Icon,
                Name = emoji.Server.Name,
                OwnerId = emoji.Server.OwnerId,
                UpdatedAt = emoji.Server.UpdatedAt,
                MembersCount = emoji.Server.ServerMembers.Count,
            };
            EmojiCreateResponse emo = new EmojiCreateResponse
            {
                Id = emoji.Id,
                Image = emoji.Image,
                Name = emoji.Name,
                server = serverRes,
                ServerId = emoji.ServerId,
                ServerMemberId = emoji.ServerMemberId
            };
            return emo;
        }
        public async Task<EmojiCreateResponse> GetByIdAsync(Guid id)
        {
            var emo = await _unitOfWork.Emojis.GetByIdAsync(id);
            if (emo == null)
            {
                throw new InvalidDataException("Emoji is not found");
            }
            return ToEmoResponse(emo);
        }

        public async Task<List<EmojiCreateResponse>> SearchAsync(Guid serverId, QueryEmoji query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if(server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var listEmo = await _unitOfWork.Emojis.SearchAsync(query.SearchTerm);
            var emoRes = new List<EmojiCreateResponse>();
            if (listEmo == null)
            {
                return new List<EmojiCreateResponse>();
            }

            listEmo = query.IsDescending ? listEmo.OrderByDescending(e => e.Name).ToList() : listEmo.OrderBy(e => e.Name).ToList();

            foreach (var emo in listEmo)
            {
                emoRes.Add(ToEmoResponse(emo));
            }
            var paginatedEmojis = emoRes
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedEmojis;
        }

        public async Task<string> UpdateAsync([FromForm]EmojiUpdateRequest request, Guid userId, IFormFile? imageFile)
        {
           var emo = await _unitOfWork.Emojis.GetByIdAsync(request.Id);
            if(emo == null) 
            {
                throw new InvalidDataException("Emoji is not found");
            }
            var server = await  _unitOfWork.Servers.GetServerOnlyAsync(emo.ServerId);

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageEmojiPermissionAsync(emo.ServerId, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to UPDATE emojis in this server");
                    }
                }
            }
            string imageUrl=emo.Image;
            if (imageFile != null)
            {
                using (var stream = imageFile.OpenReadStream())
                {
                    imageUrl = await _firebaseService.UploadAvatarAsync(stream, imageFile.FileName);
                }
            }
            emo.Name = request.Name;
            emo.Image = imageUrl;
            var updatedEmo = await _unitOfWork.Emojis.UpdateAsync(emo);
            await _hubContext.Clients.Group(emo.ServerId.ToString()).UpdateEmoji(emo.ServerId, emo.Name);
            return $"Emoji {emo.Name} has been deleted successfully";

        }
    }


}
