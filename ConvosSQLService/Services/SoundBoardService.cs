using BusinessObjects.DTOs;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR.Interfaces;
using Services.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Services
{
    public class SoundBoardService : ISoundBoardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHubContext<ServerHub, IServerHub> _hubContext;
        private readonly IPermissionService _permissionService;
        private readonly FirebaseService _firebaseService;
        public SoundBoardService(IUnitOfWork unitOfWork, IHubContext<ServerHub, IServerHub> hubContext, IPermissionService permissionService,FirebaseService firebaseService)
        {
            _unitOfWork = unitOfWork;
            _hubContext = hubContext;
            _permissionService = permissionService;
            _firebaseService = firebaseService;

        }
        public async Task<SoundBoardCreateResponse> CreateAsync([FromForm]SoundBoardCreateRequest request, IFormFile audioFile)
        {
            var server = await _unitOfWork.Servers.GetServerIncludeMembersAsync(request.ServerId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");

            }
            var member = server.ServerMembers.FirstOrDefault(sm => sm.Id.Equals(request.ServerMemberId));
            if (member == null)
            {
                throw new InvalidOperationException("Member is not existed in Server");

            }

            var serverSoundList = await _unitOfWork.SoundBoards.GetAllServerSoundAsync(server.Id);
            if (serverSoundList != null)
            {
                
                foreach (var soundb in serverSoundList)
                {
                    if (soundb.Name.Equals(request.Name))
                    {
                        throw new InvalidOperationException("Duplicate SoundBoard is Name");
                    }
                }
            }
            string audioUrl;
            using (var stream = audioFile.OpenReadStream())
            {
                audioUrl = await _firebaseService.UploadAudioAsync(stream, audioFile.FileName);
            }

            SoundBoard sound = new SoundBoard
            {
                Emoji=request.Emoji,
                ServerMemberId = request.ServerMemberId,
                Server = server,
                Name = request.Name,
                Sound=audioUrl,
                ServerId = request.ServerId,
            };
            var createdSound = await _unitOfWork.SoundBoards.CreateAsync(sound);
            await _hubContext.Clients.Group(server.Id.ToString()).CreateSoundBoard(server.Id, sound.Name);

            return ToSoundBoardResponse(createdSound);
        }
        
        private SoundBoardCreateResponse ToSoundBoardResponse(SoundBoard sound)
        {
            ServerCreateResponse serverRes = new ServerCreateResponse
            {
                CreatedAt = sound.Server.CreatedAt,
                Icon = sound.Server.Icon,
                Name = sound.Server.Name,
                OwnerId = sound.Server.OwnerId,
                UpdatedAt = sound.Server.UpdatedAt,
                MembersCount = sound.Server.ServerMembers.Count,
            };
            SoundBoardCreateResponse soundb = new SoundBoardCreateResponse
            {
                Id = sound.Id,
                Emoji = sound.Emoji,
                Name = sound.Name,
                server = serverRes,
                ServerId = sound.ServerId,
                Sound=sound.Sound,
                ServerMemberId = sound.ServerMemberId
            };
            return soundb;
        }
        public async Task<string> DeleteAsync(Guid id)
        {
            var sound = await _unitOfWork.SoundBoards.GetByIdAsync(id);
            if (sound == null)
            {
                throw new InvalidDataException("SoundBoard is not found");
            }
            
            await _unitOfWork.SoundBoards.DeleteAsync(sound);
            await _hubContext.Clients.Group(sound.ServerId.ToString()).DeleteSoundBoard(sound.ServerId, sound.Name);
            return $"SoundBoard {sound.Name} has been deleted successfully";
        }

        public async Task<List<SoundBoardCreateResponse>> GetAllAsync(Guid serverId, QuerySoundBoard query)
        {
            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }
            var serverSoundBoards = await _unitOfWork.SoundBoards.GetAllServerSoundAsync(serverId);
            if (serverSoundBoards == null)
            {
                return new List<SoundBoardCreateResponse>();
            }
            var querySound = await _unitOfWork.SoundBoards.SearchAsync(query.SearchTerm);


            var listSound = serverSoundBoards.Where(sound => querySound.Any(i => i.Id == sound.Id)).ToList();

            listSound = query.IsDescending ? listSound.OrderByDescending(e => e.Name).ToList() : listSound.OrderBy(e => e.Name).ToList();

            var tmp = new List<SoundBoardCreateResponse>();
            foreach (var sound in listSound)
            {
                tmp.Add(ToSoundBoardResponse(sound));
            }
            var paginatedSounds = tmp
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedSounds;
        }

        public async Task<SoundBoardCreateResponse> GetByIdAsync(Guid id)
        {
            var sound = await _unitOfWork.SoundBoards.GetByIdAsync(id);
            if (sound == null)
            {
                throw new InvalidDataException("SoundBoard is not found");
            }
            return ToSoundBoardResponse(sound);
        }

        public async Task<List<SoundBoardCreateResponse>> SearchAsync(Guid serverId, QuerySoundBoard query)
        {
            if (query.SearchTerm == null)
            {
                query.SearchTerm = "";
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server is not found");
            }

            var listSound = await _unitOfWork.SoundBoards.SearchAsync(query.SearchTerm);
            var soundRes = new List<SoundBoardCreateResponse>();
            if (listSound == null)
            {
                return new List<SoundBoardCreateResponse>();
            }

            listSound = query.IsDescending ? listSound.OrderByDescending(e => e.Name).ToList() : listSound.OrderBy(e => e.Name).ToList();

            foreach (var sound in listSound)
            {
                soundRes.Add(ToSoundBoardResponse(sound));
            }
            var paginatedSoundBoards = soundRes
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

            return paginatedSoundBoards;
        }

        public async Task<string> UpdateAsync(SoundBoardUpdateRequest request, Guid userId, IFormFile audioFile)
        {
            var sound = await _unitOfWork.SoundBoards.GetByIdAsync(request.Id);
            if (sound == null)
            {
                throw new InvalidDataException("SoundBoard is not found");
            }

            string audioUrl = sound.Sound;
            if (audioFile != null)
            {
                using (var stream = audioFile.OpenReadStream())
                {
                    audioUrl = await _firebaseService.UploadAudioAsync(stream, audioFile.FileName);
                }
            }
            sound.Name = request.Name;
            sound.Emoji = request.Emoji;
            sound.Sound = audioUrl;
            var updatedSound = await _unitOfWork.SoundBoards.UpdateAsync(sound);
            await _hubContext.Clients.Group(sound.ServerId.ToString()).UpdateSoundBoard(sound.ServerId, sound.Name);
            return "Updated successfully";
        }
    }
}
