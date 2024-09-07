using API.Dto;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class MessageRepository(DataContext context, IMapper mapper) : IMessageRepository
  {
    public void AddGroup(Group group)
    {
      context.Groups.Add(group);
    }

    public void AddMessage(Message message)
    {
      context.Messages.Add(message);
    }

    public void DeleteMessage(Message message)
    {
      context.Messages.Remove(message);
    }

    public async Task<Connection?> GetConnection(string connectionId)
    {
      return await context.Connections.FindAsync(connectionId);
    }

    public async Task<Group?> GetGroup(string groupName)
    {
      return await context.Groups.FindAsync(groupName);
    }

    public async Task<Group?> GetGroupForConnection(string connectionId)
    {
      return await context.Groups
        .Include(x => x.Connections)
        .Where(x => x.Connections.Any(x => x.ConnectionId == connectionId))
        .FirstOrDefaultAsync();
    }

    public async Task<Message?> GetMessage(int id)
    {
      return await context.Messages
        .Include(x => x.Connections)
        .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Group?> GetMessageGroup(string groupName)
    {
      return await context.Groups
        .Include(x => x.Connections)
        .FirstOrDefaultAsync(x => x.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
      var query = context.Messages
          .OrderByDescending(x => x.MessageSent)
          .AsQueryable();

      query = messageParams.Container switch
      {
        "Inbox" => query.Where(x => x.Recipient.UserName == messageParams.Username && !x.RecipientDeleted),
        "Outbox" => query.Where(x => x.Sender.UserName == messageParams.Username && !x.SenderDeleted),
        _ => query.Where(x => x.Recipient.UserName == messageParams.Username && x.DateRead == null && !x.RecipientDeleted)
      };
      var messages = query.ProjectTo<MessageDto>(mapper.ConfigurationProvider);
      return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesThread(string curentUsername, string recipientUsername)
    {
      var query = context.Messages
          .Where(x => x.RecipientUsername == curentUsername && !x.RecipientDeleted && x.SenderUsername == recipientUsername ||
                      x.SenderUsername == curentUsername && !x.SenderDeleted && x.RecipientUsername == recipientUsername)
          .OrderBy(x => x.MessageSent)
          .AsQueryable();

      var unreadMessages = query.Where(x => x.DateRead == null && x.RecipientUsername == curentUsername).ToList();
      if (unreadMessages.Count != 0)
      {
        unreadMessages.ForEach(x => x.DateRead = DateTime.UtcNow);
      }      
      return await query.ProjectTo<MessageDto>(mapper.ConfigurationProvider).ToListAsync();
    }

    public void RemoveConnection(Connection connection)
    {
      context.Connections.Remove(connection);
    }
  }
}
