using API.Dto;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
  public class PhotoRepository(DataContext context, IMapper mapper) : IPhotoRepository
  {
    public async Task<bool> ConfirmPhoto(int id)
    {
      var photo = await context.Photos.FindAsync(id);
      if (photo is null) throw new ArgumentNullException("Photo not found");

      var photosForUser = await context.Photos.Where(p => p.AppUserId == photo.AppUserId).ToListAsync();

      if (!photosForUser.Any(p => p.IsMain))
        photo.IsMain = true;
      photo.IsConfirmed = true;

      return true;
    }

    public async Task<List<PhotoDto>> GetPhotosForConfirm()
    {
      var query = context.Photos.Where(p => !p.IsConfirmed).AsQueryable();
      var photos = query.ProjectTo<PhotoDto>(mapper.ConfigurationProvider);
      return await photos.ToListAsync();
    }
  }
}
