using API.Dto;

namespace API.Interfaces
{
  public interface IPhotoRepository
  {
    Task<bool> ConfirmPhoto(int id);
    Task<List<PhotoDto>> GetPhotosForConfirm();
  }
}

