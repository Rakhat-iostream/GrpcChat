using Grpc.Core;
using GrpcChat.Models;
using GrpcChat.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Services
{
    public class ChatRoomsService
    {
        private readonly MessagesRepository _messagesRepository;
        private ILogger<ChatRoomsService> _logger;
        private static readonly ConcurrentBag<ChatRoom> chatRooms = new ConcurrentBag<ChatRoom>();

        public ChatRoomsService(MessagesRepository messagesRepository, ILogger<ChatRoomsService> logger)
        {
            _messagesRepository = messagesRepository;
            _logger = logger;
        }

        public async Task Join(string Name, ChatInfo chatRoom, IServerStreamWriter<Message> response)
        {
            if (!chatRooms.Any(c => c.Id.Equals(chatRoom.Id)))
                chatRooms.Add(new ChatRoom(chatRoom.Id));
            var room = chatRooms.First(c => c.Id.Equals(chatRoom.Id));

            if (room.Join(Name, response))
            {
                var chatRoomId = Guid.Parse(chatRoom.Id);
                if (!await _messagesRepository.HasUser(chatRoomId, Name))
                {
                    _logger.LogInformation($"Creating entering message for user {Name} on chat room {chatRoom.Name}");
                    await room.SendEnteringMessage(response,
                        new Message { Text = " has entered the chat!", User = Name, ChatRoomId = chatRoom.Id });
                }
                else _messagesRepository.MessagesHistory(Guid.Parse(chatRoom.Id)).Result.ForEach(m => response.WriteAsync(m));
            }
        }

        public void Remove(Guid Id, string name)
        {
            var del = chatRooms.First(x => x.Id.Equals(Id));
            del.Delete(name);
        }

        public async Task BroadcastMessageAsync(Message message) => await BroadcastMessage(message);

        private async Task BroadcastMessage(Message message)
        {
            var messageModel = new MessageModel
            {
                User = message.User,
                Text = message.Text,
                ChatRoomId = Guid.Parse(message.ChatRoomId)
            };
            var room = chatRooms.First(c => c.Id.Equals(message.ChatRoomId));
            if (await _messagesRepository.AddMessage(messageModel))
            {
                await room.SendMessage(message);
            }
            else
            {
                message.Text = "Failed to add message";
                _logger.LogError(message.Text);
            }
        }

        private async Task<KeyValuePair<string, IServerStreamWriter<Message>>?> SendMessageToSubscriber(KeyValuePair<string, IServerStreamWriter<Message>> user, Message message)
        {
            try
            {
                await user.Value.WriteAsync(message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return user;
            }
        }
    }
}
