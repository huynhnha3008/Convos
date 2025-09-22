using BusinessObjects.DTOs;
using BusinessObjects.QueryObject;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface ISoundBoardService
    {
        Task<SoundBoardCreateResponse> CreateAsync(SoundBoardCreateRequest soundCreateRequest, IFormFile audioFile);
        Task<List<SoundBoardCreateResponse>> GetAllAsync(Guid serverId, QuerySoundBoard query);

        Task<SoundBoardCreateResponse> GetByIdAsync(Guid id);

        Task<string> UpdateAsync(SoundBoardUpdateRequest request, Guid userId, IFormFile audioFile);

        Task<List<SoundBoardCreateResponse>> SearchAsync(Guid serverId, QuerySoundBoard query);

        Task<string> DeleteAsync(Guid id);
    }
}
