using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using BusinessObjects.DTOs.QuizDto;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.DTOs;

[Route("api/quizzes")]
[ApiController]

public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;


    public QuizController(IQuizService quizService)
    {
        _quizService = quizService;
    }


    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromForm] QuizCreateRequest request)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

            var result = await _quizService.CreateAsync(request, currentUserId);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
     
    }

    [Authorize]
    [HttpPost("join-quiz")]
    public async Task<IActionResult> JoinQuizAsync([FromForm] JoinQuizRequest joinQuizRequest)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

            var result = await _quizService.AddParticipantToQuiz(joinQuizRequest.quizId, currentUserId);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

    }

    [Authorize]
    [HttpPost("submit-quiz")]
    public async Task<IActionResult> SubmitQuizAsync([FromForm] SubmitQuizRequest submitQuizRequest)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
            var result = await _quizService.SubmitQuiz(submitQuizRequest.quizId,currentUserId,submitQuizRequest.score);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

    }

    [Authorize]
    [HttpPost("create-from-excel")]
    public async Task<IActionResult> CreateByExcelAsync([FromForm] CreateQuizByExelRequest request, IFormFile file)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
            var result = await _quizService.CreateQuizFromExcelAsync(file, request,currentUserId);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }

    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        try
        {
            var result = await _quizService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Internal Server Error", detail = ex.Message });
        }
    }


    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateAsync([FromBody] QuizUpdateRequest request)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

            var result = await _quizService.UpdateAsync(request, currentUserId);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }


    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);

            var message = await _quizService.DeleteAsync(id, currentUserId);
            return Ok(new { message });
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
      
    }

    [HttpGet("server/{serverId}")]
    public async Task<IActionResult> SearchAsync(Guid serverId, [FromQuery] QueryQuiz query)
    {
        try
        {
            var result = await _quizService.SearchAsync(serverId, query);
            return Ok(result);
        }
        catch (InvalidDataException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
      
    }


}
