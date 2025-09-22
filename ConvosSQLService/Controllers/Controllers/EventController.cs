using BusinessObjects.DTOs;
using BusinessObjects.DTOs.EventDto;
using BusinessObjects.DTOs.RoleDto;
using BusinessObjects.QueryObject;
using BusinessObjects.QueryObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Controllers.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult<EventCreateResponse>> CreateAsync([FromBody] EventCreateRequest combinedRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {

                var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var eve = await _eventService.CreateAsync(combinedRequest, currentUserId);

                return CreatedAtAction(nameof(GetById), new { id = eve.id }, eve);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }


        [HttpGet("servers/{id}")]
        public async Task<ActionResult> GetAllServerEvent(Guid id, [FromQuery] QueryEvent query)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var eves = await _eventService.GetAllInServerAsync(id, query);

                return Ok(eves);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        [HttpGet("channels/{id}")]
        public async Task<ActionResult> GetAllChannelEvent(Guid id,[FromQuery] QueryEvent query)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var eves = await _eventService.GetAllInChannelAsync(id, query);

                return Ok(eves);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        //[Authorize ]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var eve = await _eventService.GetByIdAsync(id);
                return Ok(eve);
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        //[Authorize ]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, [FromQuery] EventUpdateRequest request, [FromBody] List<RoleIdObject> roles)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                var updatedEmo = await _eventService.UpdateAsync(id, request);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }

        //[Authorize ]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServerEvent(Guid id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                //var currentUserId = Guid.Parse(User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value);
                await _eventService.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidDataException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ex.Message);
            }
        }
    }
}
