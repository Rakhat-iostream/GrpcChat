using Grpc.Core;
using GrpcChat.Repositories;
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

        public ChattingService(ChatRoomsService chatRoomService, ChatRepository chatRepository)
        {
            _chatRoomService = chatRoomService;
            _chatRepository = chatRepository;
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
