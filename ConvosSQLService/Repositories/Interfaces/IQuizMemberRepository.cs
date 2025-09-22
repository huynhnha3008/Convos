using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using Services.Interfaces;

namespace Repositories.Interfaces
{
    public interface IQuizMemberRepository : IGenericRepository<QuizMember>
    {
        Task<QuizMember> GetQuizMemberByPaticipantIdAsync(Guid paticipantId);
    }
}
