using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.SignalR.Interfaces
{
    public interface IQuizHub
    {
        Task DeleteQuiz(Guid serverId, string quizName);
        Task CreateQuiz(Guid serverId, string quizName);
        Task UpdateQuiz(Guid serverId, string quizName);
    }
}
