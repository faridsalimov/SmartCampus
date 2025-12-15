using Microsoft.EntityFrameworkCore;

using SmartCampus.Data.Context;
using SmartCampus.Data.Entities;
using SmartCampus.Data.Repositories.Interfaces;

namespace SmartCampus.Data.Repositories.Implementations
{
    public class MessageRepository : Repository<Message>, IMessageRepository
    {
        public MessageRepository(SmartCampusDbContext dbContext) : base(dbContext) { }

        public override async Task<IEnumerable<Message>> GetAllAsync()
        {
            return await DbSet.Include(m => m.Sender)
                              .Include(m => m.Receiver)
                              .OrderByDescending(m => m.SentDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetBySenderIdAsync(string senderId)
        {
            return await DbSet.Where(m => m.SenderId == senderId && !m.IsDeleted)
                              .Include(m => m.Sender)
                              .Include(m => m.Receiver)
                              .OrderByDescending(m => m.SentDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetByReceiverIdAsync(string receiverId)
        {
            return await DbSet.Where(m => m.ReceiverId == receiverId && !m.IsDeleted)
                              .Include(m => m.Sender)
                              .Include(m => m.Receiver)
                              .OrderByDescending(m => m.SentDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetConversationAsync(string userId1, string userId2)
        {
            return await DbSet.Where(m => !m.IsDeleted &&
                                        ((m.SenderId == userId1 && m.ReceiverId == userId2) ||
                                         (m.SenderId == userId2 && m.ReceiverId == userId1)))
                              .Include(m => m.Sender)
                              .Include(m => m.Receiver)
                              .OrderBy(m => m.SentDate)
                              .ToListAsync();
        }

        public async Task<IEnumerable<Message>> GetUnreadMessagesAsync(string receiverId)
        {
            return await DbSet.Where(m => m.ReceiverId == receiverId && !m.IsRead && !m.IsDeleted)
                              .Include(m => m.Sender)
                              .Include(m => m.Receiver)
                              .OrderByDescending(m => m.SentDate)
                              .ToListAsync();
        }

        public async Task<int> GetUnreadMessageCountAsync(string receiverId)
        {
            return await DbSet.CountAsync(m => m.ReceiverId == receiverId && !m.IsRead && !m.IsDeleted);
        }
    }
}