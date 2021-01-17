using GrpcChat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Data
{
    public class DataContext : DbContext
    {
    
        public DbSet<MessageModel> Messages { get; set; }
        public DbSet<ChatModel> ChatRooms { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
    }
}
