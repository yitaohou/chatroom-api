using ChatAPI.Application.Interfaces;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDbContext _context;

    public MessageRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _context.Messages
            .Include(m => m.User)
            .Include(m => m.Room)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Message>> GetRoomMessagesAsync(Guid roomId, int limit = 50, Guid? beforeMessageId = null)
    {
        var query = _context.Messages
            .Where(m => m.RoomId == roomId)
            .Include(m => m.User)
            .OrderByDescending(m => m.SentAt);

        if (beforeMessageId.HasValue)
        {
            var beforeMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.Id == beforeMessageId.Value);
            
            if (beforeMessage != null)
            {
                query = (IOrderedQueryable<Message>)query
                    .Where(m => m.SentAt < beforeMessage.SentAt);
            }
        }

        return await query
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Message> AddAsync(Message message)
    {
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();
        
        // Reload with navigation properties
        await _context.Entry(message)
            .Reference(m => m.User)
            .LoadAsync();
        
        return message;
    }

    public async Task DeleteAsync(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}
