using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.QueryObject
{
    public class QueryMember
    {
        public string? SearchTerm { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than or equal to 1.")]
        public int PageNumber { get; set; } = 1;
        [Range(1, int.MaxValue, ErrorMessage = "Page size must be greater than or equal to 1.")]
        public int PageSize { get; set; } = 10;
        public bool IsDescending { get; set; } = false;
        public MemberEnumSortBy SortBy { get; set; } = MemberEnumSortBy.Name;
    }

    public enum MemberEnumSortBy
    {
        Name,
        Muted,
        Deafened
    }
}
