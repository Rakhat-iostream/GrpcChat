using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Models
{
    public class MessageModel
    {
        public string User { get; set; }
        public string Text { get; set; }
        public Guid ChatRoomId { get; set; }
        public ChatModel Chat { get; set; }
    }
}
