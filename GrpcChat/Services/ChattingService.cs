using Grpc.Core;
using GrpcChat.Helpers;
using GrpcChat.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcChat.Services
{
    public class ChattingService : ChatService.ChatServiceBase
    {
        private readonly ChatRoomsService _chatRoomService;
        private readonly ChatRepository _chatRepository;
        private readonly ILogger<ChattingService> _logger;
        private readonly EventHandler<ChatEventArgs> handler = delegate { };

        public ChattingService(ChatRoomsService chatRoomService, ChatRepository chatRepository, ILogger<ChattingService> logger)
        {
            _chatRoomService = chatRoomService;
            _chatRepository = chatRepository;
            _logger = logger;
            handler = OnMessageReceived;
        }

        private async void OnMessageReceived(object sender, ChatEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Message.Text))
            {
                _logger.LogInformation($"Received message from {e.Sender}: Received date: {e.ReceivedDate.ToShortDateString()}");
                _logger.LogInformation($"Sending message to chat room {e.ChatRoomId}...");
                await _chatRoomService.BroadcastMessageAsync(e.Message);
            }
        }

        public override async Task Join(IAsyncStreamReader<Message> requestStream, IServerStreamWriter<Message> responseStream, ServerCallContext context)
        {
            if (!await requestStream.MoveNext()) return;
            var chatJoin = new Join();
            do
            {
                chatJoin.User = requestStream.Current.User;
                chatJoin.RoomId = requestStream.Current.ChatRoomId;
                await JoinChatRoom(chatJoin, responseStream);
                
                if (!string.IsNullOrEmpty(requestStream.Current.Text))
                    await _chatRoomService.BroadcastMessageAsync(requestStream.Current);
            } while (await requestStream.MoveNext());

            _chatRoomService.Remove(Guid.Parse(chatJoin.RoomId), context.Peer);
        }

        private async Task JoinChatRoom(Join request, IServerStreamWriter<Message> responseStream)
        {
            Guid chatRoomId = Guid.Parse(request.RoomId);
            var chatRoom = await _chatRepository.GetById(chatRoomId);
            if (chatRoom != null)
                await _chatRoomService.Join(request.User, chatRoom, responseStream);
        }
        public override async Task<ChatRooms> GetChatRooms(LookUp request, ServerCallContext context)
        {
            var result = new ChatRooms();
            var allChatRooms = await _chatRepository.GetAll();
            result.ChatRooms_.AddRange(allChatRooms);
            return result;
        }
    }
}
