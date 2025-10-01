using System.Security.Claims;
using ChatAPI.Application.DTOs;
using ChatAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;

    public RoomsController(RoomService roomService)
    {
        _roomService = roomService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetAllRooms()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoomDetailDto>> GetRoom(Guid id)
    {
        var room = await _roomService.GetRoomByIdAsync(id);
        
        if (room == null)
        {
            return NotFound(new { error = "Room not found" });
        }

        return Ok(room);
    }

    [HttpPost]
    public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomDto dto)
    {
        var userId = GetUserId();
        var room = await _roomService.CreateRoomAsync(dto, userId);
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, room);
    }

    [HttpPost("{id}/join")]
    public async Task<ActionResult> JoinRoom(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _roomService.JoinRoomAsync(userId, id);
            return Ok(new { message = "Successfully joined room" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/leave")]
    public async Task<ActionResult> LeaveRoom(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _roomService.LeaveRoomAsync(userId, id);
            return Ok(new { message = "Successfully left room" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRoom(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _roomService.DeleteRoomAsync(id, userId);
            return Ok(new { message = "Room deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("my-rooms")]
    public async Task<ActionResult<IEnumerable<RoomDto>>> GetMyRooms()
    {
        var userId = GetUserId();
        var rooms = await _roomService.GetUserRoomsAsync(userId);
        return Ok(rooms);
    }
}
