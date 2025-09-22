using BusinessObjects.DTOs;
using BusinessObjects.DTOs.QuizDto;
using BusinessObjects.Models;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Repositories.Interfaces;
using Services.Interfaces;
using Services.SignalR;
using Services.SignalR.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ClosedXML.Excel;
using Google;
using Repositories.impl;


namespace Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FirebaseService _firebaseService;
        private readonly IPermissionService _permissionService;
        private readonly IHubContext<QuizHub, IQuizHub> _hubContext;

        public QuizService(IUnitOfWork unitOfWork, FirebaseService firebaseService, IPermissionService permissionService, IHubContext<QuizHub, IQuizHub> hubcontext)
        {
            _unitOfWork = unitOfWork;
            _firebaseService = firebaseService;
            _permissionService = permissionService;
            _hubContext = hubcontext;
        }
        public async Task<QuizCreateResponse> CreateAsync(QuizCreateRequest request, Guid userId)
        {
            var channel = await _unitOfWork.Channels.GetByIdAsync(request.channelId);
            if(channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            var creator = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(channel.ServerId, userId);
            if(creator == null)
            {
                throw new InvalidDataException("Member is not found or input channel and creator are not in the same server");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(channel.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageQuizPermission = await HasManageQuizPermissionAsync(channel.ServerId, userId, request.channelId);
                {
                    if (!hasManageQuizPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to CREATE quiz in this server");
                    }
                }
            }

            //string quizFileUrl = "";
            //using (var stream = quizFile.OpenReadStream())
            //{
            //    quizFileUrl = await _firebaseService.UploadAvatarAsync(stream, quizFile.FileName);
            //}
            if (request.startTime == DateTime.MinValue || (request.startTime < DateTime.Now))
            {
                throw new InvalidOperationException("Invalid start time!");
            }
            if (request.duration < 1)
            {
                throw new InvalidOperationException("Invalid duration!");
            }
            var quiz = new Quiz
            {
                ChannelId = channel.Id,
                CreatedDate = DateTime.UtcNow,
                CreatorId = creator.Id,
                Description = request.description,
                Name = request.name,
                StartTime = request.startTime,
                ServerId = channel.ServerId,
                Duration = request.duration
            };
            var createdQuiz = await _unitOfWork.Quizzes.CreateAsync(quiz);

            await _hubContext.Clients.Group(server.Id.ToString()).CreateQuiz(server.Id, createdQuiz.Name);

            return ToCreateResponse(createdQuiz);
        }


        public async Task<string> AddParticipantToQuiz(Guid quizId, Guid userId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz is not found");
            }
            var participant = await _unitOfWork.ServerMembers.GetByUserIdAndServerIdIncludeRoles(userId, quiz.ServerId);
            if (participant == null)
            {
                throw new InvalidDataException("User is not found");
            }


            bool alreadyExists = (await _unitOfWork.QuizMembers
    .GetAllAsync())
    .Any(qm => qm.QuizId == quizId && qm.ParticipantId == participant.Id);

            if (alreadyExists)
                return "This user already take in this quiz";

            var quizMember = new QuizMember
            {
                Id = Guid.NewGuid(),
                QuizId = quizId,
                ParticipantId = participant.Id,
                Status = QuizStatus.in_progress
            };

             await _unitOfWork.QuizMembers.CreateAsync(quizMember);
            return "This user take in this quiz successfully";
        }

        public async Task<bool> SubmitQuiz(Guid quizId, Guid userId, int score)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(quizId);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz is not found");
            }

            var serverMember = await _unitOfWork.ServerMembers.GetByUserIdAndServerIdIncludeRoles(userId, quiz.ServerId);
            if (serverMember == null)
            {
                throw new InvalidDataException("User is not found in the server");
            }

            var participant = await _unitOfWork.QuizMembers.GetQuizMemberByPaticipantIdAsync(serverMember.Id);
            if (participant == null)
            {
                throw new InvalidDataException("Participant is not found");
            }


            participant.Score = score;
            participant.SubmittedTime = DateTime.UtcNow;
            participant.Status = QuizStatus.submitted;

            await _unitOfWork.QuizMembers.UpdateAsync(participant);
            return true;
        }

        private QuizCreateResponse ToCreateResponse(Quiz quiz)
        {
            var response = new QuizCreateResponse
            {
                channelId = quiz.ChannelId,
                createdDate = quiz.CreatedDate,
                creatorId = quiz.CreatorId,
                description = quiz.Description,
                name = quiz.Name,
                duration = quiz.Duration,
                startTime = quiz.StartTime
            };
            return response;
        }

        private async Task<bool> HasManageQuizPermissionAsync(Guid serverId, Guid userId, Guid channelId)
        {
            var userPermission = await _permissionService.GetUserChannelPermission(userId, serverId, channelId);
            if (userPermission == null)
            {
                return false;
            }
            var hasManageCategoryPermission = userPermission
                .Any(p => p.Code.Equals(PermissionEnum.MANAGE_QUIZ.ToString()));

            if (!hasManageCategoryPermission)
            {
                return false;
            }
            return true;
        }


        public async Task<string> DeleteAsync(Guid id, Guid userId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
            if(quiz == null)
            {
                throw new InvalidDataException("Quiz is not found");
            }

            // check if Quiz is on going or not 
            var isOnGoing = false;
            var endAt = quiz.StartTime.AddMinutes(quiz.Duration);
            if (quiz.StartTime < DateTime.Now && endAt > DateTime.Now)
            {
                isOnGoing = true;
            }

            if (isOnGoing)
            {
                throw new InvalidOperationException("Cannot DELETE on-going Quiz");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(quiz.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageQuizPermission = await HasManageQuizPermissionAsync(quiz.ServerId, userId, quiz.ChannelId);
                {
                    if (!hasManageQuizPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to DELETE quiz in this server");
                    }
                }
            }
            await _unitOfWork.Quizzes.DeleteAsync(quiz);
            await _hubContext.Clients.Group(server.Id.ToString()).DeleteQuiz(server.Id, quiz.Name);

            return "Delete quiz successfully";
        }

        public async Task<QuizCreateResponse> GetByIdAsync(Guid id)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id);
            if (quiz == null)
            {
                throw new InvalidDataException("Quiz is not found");
            }
            return ToCreateResponse(quiz);
        }

        public async Task<List<QuizCreateResponse>> SearchAsync(Guid serverId, QueryQuiz query)
        {
            query.SearchTerm ??= "";

            var server = await _unitOfWork.Servers.GetByIdAsync(serverId);
            if (server == null)
            {
                throw new InvalidDataException("Server not found.");
            }

            var allQuizzes = await _unitOfWork.Quizzes.GetAllAsync(); 

          
            var filteredQuizzes = allQuizzes
                .Where(q => q.ServerId == serverId &&
                            (q.Name.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                             (q.Description != null && q.Description.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            filteredQuizzes = query.IsDescending
                ? filteredQuizzes.OrderByDescending(q => q.CreatedDate).ToList()
                : filteredQuizzes.OrderBy(q => q.CreatedDate).ToList();

            var paginatedQuizzes = filteredQuizzes
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();


            var result = paginatedQuizzes
                .Select(q => ToCreateResponse(q)) 
                .ToList();

            return result;
        }


        public async Task<string> UpdateAsync(QuizUpdateRequest request, Guid userId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(request.id);
            if(quiz == null)
            {
                throw new InvalidDataException("Quiz is not found");
            }

            // check if Quiz is on going or not 
            var isOnGoing = false;
            var endAt = quiz.StartTime.AddMinutes(quiz.Duration);
            if (quiz.StartTime < DateTime.Now && endAt > DateTime.Now)
            {
                isOnGoing = true;
            }

            if (isOnGoing)
            {
                throw new InvalidOperationException("Cannot UPDATE on-going Quiz");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(quiz.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageQuizPermission = await HasManageQuizPermissionAsync(quiz.ServerId, userId, quiz.ChannelId);
                {
                    if (!hasManageQuizPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to DELETE quiz in this server");
                    }
                }
            }

            if (request.startTime != DateTime.MinValue && !(request.startTime < DateTime.Now))
            {
                quiz.StartTime = request.startTime;
            }
            if(request.duration != null && request.duration < 1)
            {
                quiz.Duration = request.duration.Value;
            }
            quiz.Name = request.name ?? quiz.Name;
            quiz.Description = request.description ?? quiz.Description;

            var updatedQuiz = await _unitOfWork.Quizzes.UpdateAsync(quiz);
            await _hubContext.Clients.Group(server.Id.ToString()).UpdateQuiz(server.Id, quiz.Name);

            return "Update successfully";
        }


        



        public async Task<CreateQuizByExelResponse> CreateQuizFromExcelAsync(IFormFile file, CreateQuizByExelRequest createQuizByExelRequest, Guid userId)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidDataException("File is empty or not provided.");
            }

            var questions = new List<QuestionDto>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var workbook = new XLWorkbook(stream))
                {
                    var worksheet = workbook.Worksheets.Worksheet(1); 
                    var rows = worksheet.RowsUsed();

                    int questionNumber = 1;

                    foreach (var row in rows.Skip(1)) 
                    {
                        var content = row.Cell(1).GetString();
                        var optionA = row.Cell(2).GetString();
                        var optionB = row.Cell(3).GetString();
                        var optionC = row.Cell(4).GetString();
                        var optionD = row.Cell(5).GetString();
                        var correctAnswer = row.Cell(6).GetString();
                        var explanation = row.Cell(7).GetString();

                        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(correctAnswer))
                        {
                            continue; 
                        }

                        var question = new QuestionDto
                        {
                            QuestionId = $"Q{questionNumber:D3}",
                            Content = content,
                            Options = new Dictionary<string, string>
                        {
                            { "A", optionA },
                            { "B", optionB },
                            { "C", optionC },
                            { "D", optionD }
                        },
                            CorrectAnswer = correctAnswer,
                            Explanation = explanation
                        };

                        questions.Add(question);
                        questionNumber++;
                    }
                }
            }
            var channel = await _unitOfWork.Channels.GetByIdAsync(createQuizByExelRequest.channelId);
            if (channel == null)
            {
                throw new InvalidDataException("Channel is not found");
            }

            var creator = await _unitOfWork.ServerMembers.GetSimpleByUserIdServerIdAsync(channel.ServerId, userId);
            if (creator == null)
            {
                throw new InvalidDataException("Member is not found or input channel and creator are not in the same server");
            }

            var server = await _unitOfWork.Servers.GetServerOnlyAsync(channel.ServerId);
            if (!userId.Equals(server.OwnerId))
            {
                var hasManageQuizPermission = await HasManageQuizPermissionAsync(channel.ServerId, userId, channel.Id);
                {
                    if (!hasManageQuizPermission)
                    {
                        throw new UnauthorizedAccessException("Permission is denied: You don't permission to CREATE quiz in this server");
                    }
                }
            }

            //string quizFileUrl = "";
            //using (var stream = quizFile.OpenReadStream())
            //{
            //    quizFileUrl = await _firebaseService.UploadAvatarAsync(stream, quizFile.FileName);
            //}
            if (createQuizByExelRequest.startTime == DateTime.MinValue || (createQuizByExelRequest.startTime < DateTime.Now))
            {
                throw new InvalidOperationException("Invalid start time!");
            }
            if (createQuizByExelRequest.duration < 1)
            {
                throw new InvalidOperationException("Invalid duration!");
            }
            var quiz = new Quiz
            {
                ChannelId = channel.Id,
                CreatedDate = DateTime.UtcNow,
                CreatorId = creator.Id,
                Description = createQuizByExelRequest.description,
                Name = createQuizByExelRequest.name,
                StartTime = createQuizByExelRequest.startTime,
                ServerId = channel.ServerId,
                Duration = createQuizByExelRequest.duration
            };
            var createdQuiz = await _unitOfWork.Quizzes.CreateAsync(quiz);

            await _hubContext.Clients.Group(server.Id.ToString()).CreateQuiz(server.Id, createdQuiz.Name);

            var response = new CreateQuizByExelResponse
            {
                QuizTitle = createQuizByExelRequest.quizTitle,
                Description = createQuizByExelRequest.description,
                Skill = createQuizByExelRequest.skill,
                TotalQuestions = questions.Count,
                Questions = questions
            };

            return response;
        }
    }
}
