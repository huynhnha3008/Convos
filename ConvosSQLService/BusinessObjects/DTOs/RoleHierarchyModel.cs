using BusinessObjects.Models;
using System;
using System.Collections.Generic;

namespace BusinessObjects.DTOs
{
    public class RoleHierarchyModel
    {
        public Guid ServerId { get; set; }
        public List<RoleWithMembersModel> Roles { get; set; }
    }

    public class RoleWithMembersModel
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public int Position { get; set; }
        public string Color { get; set; }
        public List<PermissionHierarchyModel> Permissions { get; set; }
        public List<MemberModel> Members { get; set; }
    }

    public class MemberModel
    {
        public Guid MemberId { get; set; }
        public string MemberName { get; set; }
        public string UserName { get; set; }
        public string Avatar {  get; set; }
    }

    public class PermissionHierarchyModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsGranted { get; set; }

    }
}