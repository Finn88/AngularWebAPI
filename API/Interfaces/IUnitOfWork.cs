namespace API.Interfaces
{
  public interface IUnitOfWork
  {
    IUserRepository UserRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikeRepository { get; }
    Task<bool> Complete();
    bool HasChanges();
  }
}
