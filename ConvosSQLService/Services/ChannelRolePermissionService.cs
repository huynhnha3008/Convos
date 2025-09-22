using BusinessObjects.DTOs;
using BusinessObjects.Models;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services
{
    public class ChannelRolePermissionService : IChannelRolePermissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ChannelRolePermissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ChannelRolePermission> CreateAsync(ChannelRolePermissionCreateRequest request)
        {
            var existingPermission = await _unitOfWork.ChannelRolePermissions.GetByChannelRolePermissionAsync(request.ChannelId, request.RoleId, request.PermissionId);

            if (existingPermission != null)
            {
                throw new InvalidOperationException("Permission for this role in the channel already exists.");
            }

            var channelRolePermission = new ChannelRolePermission
            {
                ChannelId = request.ChannelId,
                RoleId = request.RoleId,
                PermissionId = request.PermissionId,
                IsGranted = request.IsGranted
            };

            return await _unitOfWork.ChannelRolePermissions.CreateAsync(channelRolePermission);
        }
        private ChannelRolePermissionCreateResponse ToCRPResponse(ChannelRolePermission crp)
        {
            ChannelRolePermissionCreateResponse response = new ChannelRolePermissionCreateResponse
            {
                ChannelId = crp.ChannelId,
                RoleId = crp.RoleId,
                PermissionId = crp.PermissionId,
                IsGranted = crp.IsGranted,
            };
            return response;
        }

        public async Task DeleteAsync(Guid id)
        {
            var crp = await _unitOfWork.ChannelRolePermissions.GetByIdAsync(id);
            if (crp == null)
            {
                throw new InvalidDataException("ChannelRolePermission is not found");
            }
             await _unitOfWork.ChannelRolePermissions.DeleteAsync(crp);

        }

        public async Task<List<ChannelRolePermissionCreateResponse>> GetAllByChannelRoleId(Guid channelId, Guid roleId)
        {
            var list = await _unitOfWork.ChannelRolePermissions.GetChannelRolePermissionsByRoleIdAndChannelId(roleId, channelId);
            if (list == null)
            {
                new List<ChannelRolePermissionCreateResponse>();
            }
            var crps = new List<ChannelRolePermissionCreateResponse>();
            foreach (var crp in list)
            {
                crps.Add(ToCRPResponse(crp));
            }
            return crps;

        }

        public async Task UpdateAsync(Guid id, ChannelRolePermissionUpdateRequest request)
        {
            var crp = await _unitOfWork.ChannelRolePermissions.GetByIdAsync(id);
            if (crp == null)
            {
                throw new InvalidDataException("ChannelRolePermission is not found");
            }
            crp.IsGranted = request.isGranted;

            await _unitOfWork.ChannelRolePermissions.UpdateAsync(crp);
        }

        public async Task<ChannelRolePermissionCreateResponse> GetByIdAsync(Guid id)
        {
            var crp = await _unitOfWork.ChannelRolePermissions.GetByIdAsync(id);
            if (crp == null)
            {
                throw new InvalidDataException("ChannelRolePermission is not found");
            }
            
            return ToCRPResponse(crp);
        }
    }
}
