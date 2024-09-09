using System.Text.Json.Serialization;

namespace API.Dto
{
    public class PhotoDto {
        public int Id { get; set; }
        public string? Url { get; set; }
        public bool IsMain { get; set; }
        public bool IsConfirmed { get; set; }
    }
}
