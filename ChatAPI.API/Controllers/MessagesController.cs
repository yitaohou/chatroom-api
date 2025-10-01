using System.Security.Claims;
using ChatAPI.Application.DTOs;
using ChatAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.API.Controllers;

[ApiController]
[Route("api/rooms/{roomId}/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessagesController(MessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<ActionResult<MessageHistoryDto>> GetMessages(
        Guid roomId,
        [FromQuery] int limit = 50,
        [FromQuery] Guid? before = null)
    {
        try
        {
            var userId = GetUserId();
            var messages = await _messageService.GetRoomMessagesAsync(roomId, userId, limit, before);
            return Ok(messages);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessageController(MessageService messageService)
    {
        _messageService = messageService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MessageDto>> GetMessage(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var message = await _messageService.GetMessageByIdAsync(id, userId);
            
            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            return Ok(message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
