using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Models
{
    public class ChatModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public List<MessageModel> Messages { get; set; }
    }
}
