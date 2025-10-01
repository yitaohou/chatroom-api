using System.Security.Claims;
using ChatAPI.Application.DTOs;
using ChatAPI.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatAPI.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MessageService _messageService;
    private readonly RoomService _roomService;

    public ChatHub(MessageService messageService, RoomService roomService)
    {
        _messageService = messageService;
        _roomService = roomService;
    }

    private Guid GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    // Called when client connects
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        Console.WriteLine($"User {userId} connected with connection ID: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    // Called when client disconnects
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        Console.WriteLine($"User {userId} disconnected");
        await base.OnDisconnectedAsync(exception);
    }

    // Join a room (SignalR group)
    public async Task JoinRoom(string roomId)
    {
        var userId = GetUserId();
        var roomGuid = Guid.Parse(roomId);

        // Verify user is member of the room
        var isMember = await _roomService.GetRoomByIdAsync(roomGuid);
        if (isMember == null)
        {
            throw new HubException("Room not found");
        }

        // Add connection to SignalR group
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        
        // Notify others in the room
        await Clients.OthersInGroup(roomId).SendAsync("UserJoined", new
        {
            userId = userId.ToString(),
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });

        Console.WriteLine($"User {userId} joined room {roomId}");
    }

    // Leave a room (SignalR group)
    public async Task LeaveRoom(string roomId)
    {
        var userId = GetUserId();

        // Remove connection from SignalR group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
        
        // Notify others in the room
        await Clients.OthersInGroup(roomId).SendAsync("UserLeft", new
        {
            userId = userId.ToString(),
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });

        Console.WriteLine($"User {userId} left room {roomId}");
    }

    // Send a message to a room
    public async Task SendMessage(string roomId, string content)
    {
        var userId = GetUserId();
        var roomGuid = Guid.Parse(roomId);

        // Save message to database
        var messageDto = new SendMessageDto
        {
            RoomId = roomGuid,
            Content = content
        };

        try
        {
            var savedMessage = await _messageService.SendMessageAsync(messageDto, userId);

            // Broadcast message to all clients in the room (including sender)
            await Clients.Group(roomId).SendAsync("ReceiveMessage", savedMessage);

            Console.WriteLine($"User {userId} sent message to room {roomId}");
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new HubException(ex.Message);
        }
    }

    // Typing indicator
    public async Task SendTypingIndicator(string roomId, bool isTyping)
    {
        var userId = GetUserId();

        // Notify others in the room (not the sender)
        await Clients.OthersInGroup(roomId).SendAsync("UserTyping", new
        {
            userId = userId.ToString(),
            isTyping,
            timestamp = DateTime.UtcNow
        });
    }
}
