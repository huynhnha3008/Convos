using BusinessObjects.DTOs;
using BusinessObjects.DTOs.QuizDto;
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
    public interface IQuizService
    {
        Task<QuizCreateResponse> CreateAsync(QuizCreateRequest request, Guid userId);

        Task<QuizCreateResponse> GetByIdAsync(Guid id);

        Task<string> UpdateAsync(QuizUpdateRequest request, Guid userId);

        Task<List<QuizCreateResponse>> SearchAsync(Guid serverId, QueryQuiz query);

        Task<CreateQuizByExelResponse> CreateQuizFromExcelAsync(IFormFile file, CreateQuizByExelRequest createQuizByExelRequest, Guid userId);

        Task<string> DeleteAsync(Guid id, Guid userId);

        Task<string> AddParticipantToQuiz(Guid quizId, Guid participantId);

        Task<bool> SubmitQuiz(Guid quizId, Guid userId, int score);
    }
}
