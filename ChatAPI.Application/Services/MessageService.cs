using ChatAPI.Application.DTOs;
using ChatAPI.Application.Interfaces;
using ChatAPI.Domain.Entities;

namespace ChatAPI.Application.Services;

public class MessageService
{
    private readonly IMessageRepository _messageRepository;
    private readonly IRoomRepository _roomRepository;

    public MessageService(IMessageRepository messageRepository, IRoomRepository roomRepository)
    {
        _messageRepository = messageRepository;
        _roomRepository = roomRepository;
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageDto dto, Guid userId)
    {
        // Verify user is member of the room
        var isMember = await _roomRepository.IsUserMemberAsync(userId, dto.RoomId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Must be a member of the room to send messages");
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            Content = dto.Content,
            SentAt = DateTime.UtcNow,
            UserId = userId,
            RoomId = dto.RoomId
        };

        var savedMessage = await _messageRepository.AddAsync(message);

        return new MessageDto
        {
            Id = savedMessage.Id,
            Content = savedMessage.Content,
            SentAt = savedMessage.SentAt,
            RoomId = savedMessage.RoomId,
            User = new UserDto
            {
                Id = savedMessage.User.Id,
                Username = savedMessage.User.Username,
                Email = savedMessage.User.Email,
                CreatedAt = savedMessage.User.CreatedAt
            }
        };
    }

    public async Task<MessageHistoryDto> GetRoomMessagesAsync(Guid roomId, Guid userId, int limit = 50, Guid? beforeMessageId = null)
    {
        // Verify user is member of the room
        var isMember = await _roomRepository.IsUserMemberAsync(userId, roomId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Must be a member of the room to view messages");
        }

        var messages = await _messageRepository.GetRoomMessagesAsync(roomId, limit + 1, beforeMessageId);
        var messageList = messages.ToList();

        var hasMore = messageList.Count > limit;
        if (hasMore)
        {
            messageList = messageList.Take(limit).ToList();
        }

        return new MessageHistoryDto
        {
            Messages = messageList.Select(m => new MessageDto
            {
                Id = m.Id,
                Content = m.Content,
                SentAt = m.SentAt,
                RoomId = m.RoomId,
                User = new UserDto
                {
                    Id = m.User.Id,
                    Username = m.User.Username,
                    Email = m.User.Email,
                    CreatedAt = m.User.CreatedAt
                }
            }).ToList(),
            HasMore = hasMore
        };
    }

    public async Task<MessageDto?> GetMessageByIdAsync(Guid messageId, Guid userId)
    {
        var message = await _messageRepository.GetByIdAsync(messageId);
        
        if (message == null)
        {
            return null;
        }

        // Verify user is member of the room
        var isMember = await _roomRepository.IsUserMemberAsync(userId, message.RoomId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("Must be a member of the room to view this message");
        }

        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            RoomId = message.RoomId,
            User = new UserDto
            {
                Id = message.User.Id,
                Username = message.User.Username,
                Email = message.User.Email,
                CreatedAt = message.User.CreatedAt
            }
        };
    }
}
