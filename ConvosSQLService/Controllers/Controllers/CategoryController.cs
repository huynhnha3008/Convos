using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using BusinessObjects.QueryObject;
using BusinessObjects.DTOs;
using BusinessObjects;

namespace Services.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IServerService _serverService;


        public CategoryController(ICategoryService categoryService, IServerService serverService)
        {
            _categoryService = categoryService;
            _serverService = serverService;
           
        }
        [Authorize  ]
        [HttpPost]
        public async Task<ActionResult<CategoryDetailResponse>> Create([FromBody] CategoryCreateRequest request)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                // custom category creation
                int serverType = 0;
                var category = await _categoryService.CreateAsync(request,currentUserId, serverType);

                return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }


        [Authorize  ]
        [HttpGet("server/{id:guid}")]
        public async Task<ActionResult<List<CategoryDetailResponse>>> GetAll(Guid id, [FromQuery] QueryCategory query)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var categoryList = await _categoryService.GetAllAsync(id, query);
                return Ok(categoryList);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
           
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var category = await _categoryService.GetByIdAsync(id);
                return Ok(category);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<string>> Update(Guid id, [Required] [FromBody] CategoryUpdateRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var result = await _categoryService.UpdateAsync(id, currentUserId, request);
                return Ok(result);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [Authorize ]
        [HttpPost("{id}/change-category-position")]
        public async Task<IActionResult> ChangeCategoryPosition(Guid id, [FromQuery] ChannelUpdatePositionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
               
                var message = await _categoryService.ChangePositionAsync(id, request.newPosition, currentUserId);

                return Ok(message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }

        }

        [Authorize  ]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Delete(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                await _categoryService.DeleteAsync(id, currentUserId);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
