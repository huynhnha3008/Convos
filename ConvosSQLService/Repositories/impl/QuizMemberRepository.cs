using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using Services.impl;

namespace Repositories.impl
{
    public class QuizMemberRepository : GenericRepository<QuizMember>, IQuizMemberRepository
    {
        private readonly ConvosDbContext _context;
        public QuizMemberRepository(ConvosDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<QuizMember> GetQuizMemberByPaticipantIdAsync(Guid paticipantId)
        {
            var rs = await _context.QuizMembers.FirstOrDefaultAsync(q => q.ParticipantId.Equals(paticipantId));
            return rs;
        }

    }
}
