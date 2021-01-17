using GrpcChat.Data;
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
    }
}
