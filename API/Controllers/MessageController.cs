using API.Dto;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
  [Authorize]
  public class MessageController(IUnitOfWork unitOfWork,
      IUserRepository userRepository, IMapper mapper) : BaseApiController
  {
    [HttpPost()]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
      var username = User.GetUserName();
      if (username == createMessageDto.RecipientUsername.ToLower())
        return BadRequest("You cant message yourself");

      var sender = await userRepository.GetUserByNameAsync(username);
      var recipient = await userRepository.GetUserByNameAsync(createMessageDto.RecipientUsername);

      if (recipient == null || sender == null || recipient == null) return BadRequest("Cant send message at this time");

      var message = new Message
      {
        Sender = sender,
        Recipient = recipient,
        SenderUsername = username,
        RecipientUsername = recipient.UserName!,
        Content = createMessageDto.Content
      };

      unitOfWork.MessageRepository.AddMessage(message);
      if (await unitOfWork.Complete()) return Ok(mapper.Map<MessageDto>(message));

      return BadRequest("Send message failed");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams)
    {
      messageParams.Username = User.GetUserName();
      var messages = await unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
      Response.AddPaginationHeader(messages);
      return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
    {
      var currentUsername = User.GetUserName();
      var messages = await unitOfWork.MessageRepository.GetMessagesThread(currentUsername, username);
      return Ok(messages);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id)
    {
      var message = await unitOfWork.MessageRepository.GetMessage(id);
      if (message == null) return BadRequest("Message not found");


      var currentUsername = User.GetUserName();
      if (!(message.SenderUsername == currentUsername || message.RecipientUsername == currentUsername)) return Forbid();
      if (message.SenderUsername == currentUsername) message.SenderDeleted = true;
      if (message.RecipientUsername == currentUsername) message.RecipientDeleted = true;

      if (message is { SenderDeleted: true, RecipientDeleted: true })
      {
        unitOfWork.MessageRepository.DeleteMessage(message);
      }

      if (await unitOfWork.Complete()) return Ok();

      return BadRequest("Not possible to delete message");
    }

  }
}
