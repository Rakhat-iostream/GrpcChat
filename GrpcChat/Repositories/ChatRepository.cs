using GrpcChat.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Repositories
{
    public class ChatRepository
    {
        private readonly DataContext _context;

        public ChatRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<ChatInfo>> GetAll()
        {
            return await _context.ChatRooms.Select(c => new ChatInfo { Id = c.Id.ToString(), Name = c.Name }).ToListAsync();
        }

        public async Task<ChatInfo> GetById(Guid id)
        {
            var chatRoom = await _context.ChatRooms.FindAsync(id);
            return new ChatInfo { Id = chatRoom.Id.ToString(), Name = chatRoom.Name };
        }

    }
}
