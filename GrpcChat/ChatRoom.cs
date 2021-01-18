using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat
{
    public class ChatRoom
    {
        public string Id { get; set; }
        private readonly ConcurrentDictionary<string, IServerStreamWriter<Message>> 
            users = new ConcurrentDictionary<string, IServerStreamWriter<Message>>();

        public ChatRoom(string id)
        {
            this.Id = id;
            users = new ConcurrentDictionary<string, IServerStreamWriter<Message>>();
        }

        public bool Join(string name, IServerStreamWriter<Message> response)
        {
            return  users.TryAdd(name, response);
        }
        public bool Delete(string name)
        {
            return users.TryRemove(name, out _);
        }
        public async Task SendEnteringMessage(IServerStreamWriter<Message> response, Message message)
        {
            foreach (var val in users.Values)
                await val.WriteAsync(message);
        }

        public async Task SendMessage(Message message)
        {
            foreach (var pair in users.Where(x => x.Key != message.User))
            {
                await pair.Value.WriteAsync(message);
            }
        }

    }
}
