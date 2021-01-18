using GrpcChat.Data;
using GrpcChat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Repositories
{
    public class MessagesRepository
    {
        private readonly DataContext _context;

        public MessagesRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<List<Message>> MessagesHistory(Guid Id)
        {
            return await _context.Messages.Where(x => x.ChatRoomId.Equals(Id)).Select(
                x => new Message { Text = x.Text, User = x.User, ChatRoomId = x.ChatRoomId.ToString() }).ToListAsync();
        }

        public async Task<bool> AddMessage(MessageModel message)
        {
            _context.Messages.Add(message);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> HasUser(Guid Id, string Name)
        {
            return await _context.Messages.AnyAsync(x => x.User.Equals(Name) && x.ChatRoomId.Equals(Id));
        }
    }
}
