using ChatAPI.Domain.Entities;

namespace ChatAPI.Application.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(Guid id);
    Task<IEnumerable<Message>> GetRoomMessagesAsync(Guid roomId, int limit = 50, Guid? beforeMessageId = null);
    Task<Message> AddAsync(Message message);
    Task DeleteAsync(Guid id);
}
