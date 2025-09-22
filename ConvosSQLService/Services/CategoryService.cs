using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;
using Services.SignalR.Interfaces;
using Services.SignalR;
using Repositories.Interfaces;
using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using BusinessObjects.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BusinessObjects.DTOs.RealTImeDto;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IChannelService _channelService;
        private readonly IHubContext<ServerHub, IServerHub> _hubContext;
        private readonly IPermissionService _permissionService;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IChannelService channelService, IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService)
        {
            _channelService = channelService;
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _permissionService = permissionService;
        }
        public async Task<Category> CreateAsync(CategoryCreateRequest Category, Guid userId, int serverType)
        {
            Category cate;
            int ServerCategorySize;
            var server = await _unitOfWork.Servers.GetServerIncludeMembersCateChannelsAsync(Category.ServerId);
            if (server == null)
            {
                throw new InvalidOperationException("Server is not found");
            }
            if (server.Categories == null || !server.Categories.Any())
            {
                ServerCategorySize = 1;
            }
            else
            {
                ServerCategorySize = server.Categories.Count() + 1;
            }


            if (Category.Name.Equals("Text Channels"))
            {
                cate = CreateCategory(Category.Name, Category.ServerId, ServerCategorySize);

                Category createdCategory = await _unitOfWork.Categories.CreateAsync(cate);

                switch (serverType)
                {
                    // Study Group Server
                    case 2:
                        Channel studyTips = await CreateTextChannel("study-tips", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(studyTips);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);

                        Channel qna = await CreateTextChannel("Q-and-A", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(qna);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        break;

                    // Peer Collaboration Server
                    case 3:
                        Channel projectIdeas = await CreateTextChannel("project-ideas", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(projectIdeas);
                        // phai update lien tuc de cap nhat position ~~ optimize sau
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);

                        Channel peer_help = await CreateTextChannel("peer-help", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(peer_help);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        break;

                    // class server
                    case 4:
                        await CreateTextChannel("meeting-plans", Category.ServerId, createdCategory.Id);
                        await CreateTextChannel("off-topic", Category.ServerId, createdCategory.Id);
                        break;
                }

                await CreateTextChannel("general", Category.ServerId, createdCategory.Id);
            }
            else if (Category.Name.Equals("Voice Channels"))
            {
                cate = CreateCategory("Voice Channels", Category.ServerId, ServerCategorySize);
                Category createdCategory = await _unitOfWork.Categories.CreateAsync(cate);

                switch (serverType)
                {

                    // default server
                    case 1:
                        await CreateVoiceChannel("General", Category.ServerId, createdCategory.Id);
                        break;

                    //Study Group Serverr
                    case 2:
                        Channel groupStudy = await CreateVoiceChannel("Group Study Room", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(groupStudy);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        Channel quickHelp = await CreateVoiceChannel("Quick Help Room", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(quickHelp);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        break;

                    // Peer Collaboration Server
                    case 3:
                        Channel collabRoom = await CreateVoiceChannel("Collab Room ", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(collabRoom);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        Channel projecttalk = await CreateVoiceChannel("Project Talk", Category.ServerId, createdCategory.Id);
                        createdCategory.Channels.Add(projecttalk);
                        await _unitOfWork.Categories.UpdateAsync(createdCategory);
                        break;

                    // class server
                    case 4:
                        await CreateVoiceChannel("Lounge", Category.ServerId, createdCategory.Id);
                        await CreateVoiceChannel("Meeting Room 1", Category.ServerId, createdCategory.Id);
                        await CreateVoiceChannel("Meeting Room 2", Category.ServerId, createdCategory.Id);
                        break;
                }
            }
            else if (Category.Name.Equals("Information"))
            {
                // Handle default or information category
                cate = CreateCategory("Information", Category.ServerId, ServerCategorySize);
                Category createdCategory = await _unitOfWork.Categories.CreateAsync(cate);

                await CreateTextChannel("welcome-and-rules", Category.ServerId, createdCategory.Id);

                await CreateTextChannel("announcements", Category.ServerId, createdCategory.Id);

                await CreateTextChannel("resources", Category.ServerId, createdCategory.Id);
            }
            // custom category creating
            else
            {
                ServerMember creatorMem = null;
                foreach (var m in server.ServerMembers)
                {
                    if (m.UserId.Equals(userId))
                    {
                        creatorMem = m;
                    }
                }

                if (creatorMem == null)
                {
                    throw new UnauthorizedAccessException("You are not belong to this server");
                }
                if (!userId.Equals(server.OwnerId))
                {
                    var hasManageChannelPermission = await HasManageCategoryPermissionAsync(Category.ServerId, userId);
                    {
                        if (!hasManageChannelPermission)
                        {
                            throw new UnauthorizedAccessException("Permission is denied: You don't have permission to CREATE category in this server");
                        }
                    }
                }


                cate = CreateCategory(Category.Name, Category.ServerId, ServerCategorySize);
                cate.IsPrivate = Category.IsPrivate;
                var createdCate = await _unitOfWork.Categories.CreateAsync(cate);

                //set creator
                createdCate.CreatorId = creatorMem.Id;
                await _unitOfWork.Categories.UpdateAsync(createdCate);
                await _hubContext.Clients.Group(server.Id.ToString()).CreateCategory(server.Id, ToCategoryRealtimeResponse(createdCate));

            }

            return cate;
        }

        private CategoryRealtimeResponse ToCategoryRealtimeResponse (Category category)
        {
            var rs = new CategoryRealtimeResponse
            {
                CreatedAt = category.CreatedAt,
                CreatorId = category.CreatorId,
                Id = category.Id,
                IsPrivate = category.IsPrivate,
                Name = category.Name,
                Position = category.Position,
                UpdatedAt = category.UpdatedAt,
            };
            var channels = new List<ChannelRealtimeResponse>();
            if(category.Channels != null)
            {
                foreach (var channel in category.Channels)
                {
                    channels.Add(ToChannelRealtimeResponse(channel));
                }
                rs.Channels = channels;
            }
           
            return rs;
        }

        private ChannelRealtimeResponse ToChannelRealtimeResponse(Channel channel)
        {
            var rs = new ChannelRealtimeResponse
            {
                Id = channel.Id,
                CreatedAt = channel.CreatedAt,
                IsPrivate = channel.IsPrivate,
                Name = channel.Name,
                Position = channel.Position,
                Type = (int)channel.Type,
                UpdatedAt = channel.UpdatedAt,
            };
            if (channel.CategoryId != null)
            {
                rs.CategoryId = channel.CategoryId.Value;
            }
            return rs;
        }

        private async Task<bool> HasManageCategoryPermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageCategoryPermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_CATEGORIES.ToString()));

            if (!hasManageCategoryPermission)
            {
                return false;
            }
            return true;
        }

        private Category CreateCategory(string name, Guid serverId, int position)
        {
            return new Category
            {
                Name = name,
                ServerId = serverId,
                CreatedAt = DateTime.Now,
                Position = position,
                UpdatedAt = DateTime.Now
            };
        }

        private async Task<Channel> CreateTextChannel(string channelName, Guid serverId, Guid categoryId)
        {
            Channel channelReq = new Channel
            {
                Name = channelName,
                ServerId = serverId,
                CategoryId = categoryId,
                IsPrivate = false,
                Type = ChannelType.Text
            };

            return await _channelService.CreateAutoAsync(channelReq);
        }

        private async Task<Channel> CreateVoiceChannel(string channelName, Guid serverId, Guid categoryId)
        {
            Channel channelReq = new Channel
            {
                Name = channelName,
                ServerId = serverId,
                CategoryId = categoryId,
                IsPrivate = false,
                Type = ChannelType.Voice
            };
            return await _channelService.CreateAutoAsync(channelReq);
        }

        private async Task<bool> HasManageChannelPermissionAsync(Guid serverId, Guid userId)
        {
            var userPermission = await _permissionService.GetUserGlobalPermission(userId, serverId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageChannelPermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_CHANNELS.ToString()));

            if (!hasManageChannelPermission)
            {
                return false;
            }
            return true;
        }

        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null)
                throw new InvalidDataException("Category is not found");

            var server = await _unitOfWork.Servers.GetServerIncludeCateChannelAsync(category.ServerId);
            if (server == null)
                throw new InvalidDataException("Server is not found");

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageCategoryPermission = await HasManageCategoryPermissionAsync(server.Id, userId);
                if (!hasManageCategoryPermission)
                    throw new UnauthorizedAccessException("You don't have permission to DELETE category in this server");
            }

            // delete all related channels
            foreach (var channel in category.Channels)
            {

                if (!userId.Equals(server.OwnerId))
                {
                    var hasManageCategoryPermission = await HasManageChannelPermissionAsync(server.Id, userId);
                    if (!hasManageCategoryPermission) { break; }
                }

                await _unitOfWork.Channels.DeleteAsync(channel);
            }

            var removed = server.Categories.Remove(category);
            if (!removed)
                throw new InvalidOperationException("Category could not be removed from server list.");


            int position = 1;
            foreach (var existingCategory in server.Categories.OrderBy(c => c.Position))
            {
                existingCategory.Position = position++;
            }


            await _unitOfWork.Servers.UpdateAsync(server);
            // await _unitOfWork.Categories.DeleteAsync(category);

            await _hubContext.Clients.Group(server.Id.ToString()).DeleteCategory(server.Id, category.Id.ToString());

            return "Category is deleted successfully";
        }



        public async Task<List<CategoryDetailResponse>> GetAllAsync(Guid serverId, QueryCategory query)
        {
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                return await SearchAsync(serverId, query);
            }

            var cates = await _unitOfWork.Categories.GetAllAsync(serverId);
            if (cates == null || cates.Count == 0)
            {
                return new List<CategoryDetailResponse>();
            }

            switch (query.SortBy.ToString())
            {
                case "Name":
                    cates = query.IsDescending ? cates.OrderByDescending(c => c.Name).ToList() : cates.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    cates = query.IsDescending ? cates.OrderByDescending(c => c.CreatedAt).ToList() : cates.OrderBy(c => c.CreatedAt).ToList();
                    break;
                default:
                    cates = cates.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedCates = cates
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<CategoryDetailResponse>();
            foreach (var cate in paginatedCates)
            {
                rs.Add(ToCategoryDetailAsync(cate));
            }
            return rs;
        }


        public async Task<CategoryDetailResponse> GetByIdAsync(Guid id)
        {
            var cate = await _unitOfWork.Categories.GetByIdAsync(id);
            if (cate == null)
            {
                throw new InvalidDataException("Category is not found");
            }

            return ToCategoryDetailAsync(cate);
        }

        public async Task<List<CategoryDetailResponse>> SearchAsync(Guid serverId, QueryCategory query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }
            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var cateList = await _unitOfWork.Categories.SearchAsync(query.SearchTerm);
            if (cateList == null)
            {
                return new List<CategoryDetailResponse>();
            }

            var cates = cateList.Where(c => c.ServerId.Equals(serverId)).ToList();
            switch (query.SortBy.ToString())
            {
                case "Name":
                    cates = query.IsDescending ? cates.OrderByDescending(c => c.Name).ToList() : cates.OrderBy(c => c.Name).ToList();
                    break;
                case "CreateAt":
                    cates = query.IsDescending ? cates.OrderByDescending(c => c.CreatedAt).ToList() : cates.OrderBy(c => c.CreatedAt).ToList();
                    break;
                default:
                    cates = cates.OrderBy(s => s.Name).ToList();
                    break;
            }

            var paginatedCates = cates
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            var rs = new List<CategoryDetailResponse>();
            foreach (var cate in paginatedCates)
            {
                rs.Add(ToCategoryDetailAsync(cate));
            }
            return rs;
        }

        public async Task<string> UpdateAsync(Guid id, Guid userId, CategoryUpdateRequest request)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);

            if (category == null)
            {
                throw new InvalidDataException("Category is not found");
            }
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(category.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageChannelPermission = await HasManageCategoryPermissionAsync(category.ServerId, userId);
                {
                    if (!hasManageChannelPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't have permission to UPDATE category in this server");
                    }
                }
            }

            if (!string.IsNullOrEmpty(request.Name))
            {
                category.Name = request.Name;
            }

            // check private - proceed private channels
            category.IsPrivate = request.IsPrivate;

            // only accept status isPrivate from 
            if (request.IsPrivate)
            {
                foreach (var channel in category.Channels)
                {
                    await _channelService.SetChannelPrivacyAsync(channel.Id, category.IsPrivate); // include signalR inside already
                }
            }


            category.UpdatedAt = DateTime.UtcNow;
            var res = await _unitOfWork.Categories.UpdateAsync(category);
            await _hubContext.Clients.Group(server.Id.ToString()).UpdateCategory(server.Id, ToCategoryRealtimeResponse(res));
            return "Category updated successfully";
        }

        private CategoryDetailResponse ToCategoryDetailAsync(Category category)
        {
            List<CategoryChannelResponse> channels = new List<CategoryChannelResponse>();
            foreach (var channel in category.Channels)
            {
                CategoryChannelResponse channelResponse = new CategoryChannelResponse
                {
                    Id = channel.Id,
                    UpdatedAt = channel.UpdatedAt,
                    CreatedAt = channel.CreatedAt,
                    Position = channel.Position,
                    IsPrivate = channel.IsPrivate,
                    Name = channel.Name,
                    Type = channel.Type.ToString()
                };
                channels.Add(channelResponse);
            }
            CategoryDetailResponse response = new CategoryDetailResponse
            {
                Id = category.Id,
                Name = category.Name,
                Position = category.Position,
                CreatedAt = category.CreatedAt,
                ServerId = category.ServerId,
                UpdatedAt = category.UpdatedAt,
                Channels = channels
            };

            return response;
        }

        public async Task<string> ChangePositionAsync(Guid id, int newPosition, Guid userId)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id)
                ?? throw new InvalidDataException("Category not found");

            if (newPosition == category.Position)
            {
                throw new InvalidDataException("New position is the same as old position.");
            }

            var server = await _unitOfWork.Servers.GetByIdAsync(category.ServerId)
                ?? throw new InvalidDataException("Category does not belong to any server");

            if (!userId.Equals(server.OwnerId))
            {
                var hasManageCategoryPermission = await HasManageCategoryPermissionAsync(category.ServerId, userId);
                if (!hasManageCategoryPermission)
                {
                    throw new UnauthorizedAccessException("Permission is denied: You don't have permission to update this category in this server");
                }
            }

            var categories = server.Categories.OrderBy(c => c.Position).ToList();

            if (newPosition < 0)
            {
                throw new InvalidOperationException("Invalid input position");
            }

            if (newPosition == 0)
            {
                newPosition = 1;
            }
            if (newPosition >= categories.Count())
            {
                newPosition = categories.Count();
            }

            if (category.Position < newPosition)
            {
                foreach (var c in categories.Where(c => c.Position > category.Position && c.Position <= newPosition))
                {
                    c.Position--;
                    await _unitOfWork.Categories.UpdateAsync(c);
                }
            }
            else if (category.Position > newPosition)
            {
                foreach (var c in categories.Where(c => c.Position >= newPosition && c.Position < category.Position))
                {
                    c.Position++;
                    await _unitOfWork.Categories.UpdateAsync(c);
                }
            }

            category.Position = newPosition;
            category.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Categories.UpdateAsync(category);

            await _hubContext.Clients.Group(server.Id.ToString()).ChangeCategoryPosition(server.Id, category.Id, newPosition);

            return "Category position has been updated successfully.";
        }

    }
}

