using API.Data;
using API.Dto;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace API.SignaR
{
  [Authorize]
  public class MessageHub(IUnitOfWork unitOfWork, IMapper mapper,
    IHubContext<PresenceHub> presenceHub) : Hub
  {
    public override async Task OnConnectedAsync()
    {
      var httpContext = Context.GetHttpContext();
      var otherUser = httpContext?.Request.Query["user"];

      var groupName = GetGroupName(Context.User?.GetUserName()!, otherUser!);
      await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
      var group = await AddToGroup(groupName);
      await Clients.Group(groupName).SendAsync("UpdatedGroup", group);

      var messages = await unitOfWork.MessageRepository.GetMessagesThread(Context.User?.GetUserName()!, otherUser!);
      if(unitOfWork.HasChanges())
        await unitOfWork.Complete();

      await Clients.Caller.SendAsync("RecieveMessageThread", messages);
    }

    public override async Task OnDisconnectedAsync(Exception? ex)
    {
      var group = await RemoveFromMessageGroup();
      await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
      await base.OnDisconnectedAsync(ex);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
      var username = Context.User?.GetUserName() ?? throw new Exception();

      var sender = await unitOfWork.UserRepository.GetUserByNameAsync(username);
      var recipient = await unitOfWork.UserRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = username,
        RecipientUsername = recipient.UserName!,
        Content = createMessageDto.Content
      };

      var groupName = GetGroupName(sender.UserName, recipient.UserName);
      var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
      if (group is not null && group.Connections.Any(x=>x.UserName == recipient.UserName))
      {
        message.DateRead = DateTime.UtcNow;
      }
      else
      {
        var connections = await PresenceTracker.GetConnectionsForUser(recipient.UserName);
        if (connections != null && connections?.Count != null)
        {
          await presenceHub.Clients.Clients(connections)
            .SendAsync("NewMessageRecieved",
            new {username = sender.UserName, knownAs = sender.KnownAs });
        }
      }

      unitOfWork.MessageRepository.AddMessage(message);
      if (await unitOfWork.Complete())
      {
        await Clients.Group(groupName).SendAsync("NewMessage", mapper.Map<MessageDto>(message));
      }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
      var username = Context.User?.GetUserName() ?? throw new Exception();
      var group = await unitOfWork.MessageRepository.GetMessageGroup(groupName);
      var connection = new Connection { ConnectionId = Context.ConnectionId, UserName = username };

      if (group is null)
      {
        group = new Group { Name = groupName };
        unitOfWork.MessageRepository.AddGroup(group);
      }
      group.Connections.Add(connection);

      if( await unitOfWork.Complete()) return group;
      throw new HubException();
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
      var group = await unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
      var connection = group?.Connections.FirstOrDefault(x=>x.ConnectionId == Context.ConnectionId);
      if (connection is not null && group is not null)
      {
        unitOfWork.MessageRepository.RemoveConnection(connection);
        if (await unitOfWork.Complete()) return group;
      }
      throw new Exception();
    }

    private string GetGroupName(string caller, string other)
    {
      var compare = string.CompareOrdinal(caller, other) < 0;
      return compare ? $"{caller}-{other}" : $"{other}-{caller}";
    }
  }
}
