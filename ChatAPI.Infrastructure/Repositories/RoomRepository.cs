using ChatAPI.Application.Interfaces;
using ChatAPI.Domain.Entities;
using ChatAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatAPI.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly ChatDbContext _context;

    public RoomRepository(ChatDbContext context)
    {
        _context = context;
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        return await _context.Rooms
            .Include(r => r.CreatedBy)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Room?> GetByIdWithMembersAsync(Guid id)
    {
        return await _context.Rooms
            .Include(r => r.CreatedBy)
            .Include(r => r.UserRooms)
                .ThenInclude(ur => ur.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        return await _context.Rooms
            .Include(r => r.CreatedBy)
            .ToListAsync();
    }

    public async Task<IEnumerable<Room>> GetUserRoomsAsync(Guid userId)
    {
        return await _context.UserRooms
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Room)
                .ThenInclude(r => r.CreatedBy)
            .Select(ur => ur.Room)
            .ToListAsync();
    }

    public async Task<Room> AddAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
        return room;
    }

    public async Task UpdateAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var room = await GetByIdAsync(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> IsUserMemberAsync(Guid userId, Guid roomId)
    {
        return await _context.UserRooms
            .AnyAsync(ur => ur.UserId == userId && ur.RoomId == roomId);
    }

    public async Task AddUserToRoomAsync(Guid userId, Guid roomId)
    {
        var userRoom = new UserRoom
        {
            UserId = userId,
            RoomId = roomId,
            JoinedAt = DateTime.UtcNow
        };
        _context.UserRooms.Add(userRoom);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUserFromRoomAsync(Guid userId, Guid roomId)
    {
        var userRoom = await _context.UserRooms
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoomId == roomId);
        
        if (userRoom != null)
        {
            _context.UserRooms.Remove(userRoom);
            await _context.SaveChangesAsync();
        }
    }
}
