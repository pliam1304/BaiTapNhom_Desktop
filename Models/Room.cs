namespace EduPath.Avalonia.Models
{
    public class Room
    {
        public string RoomId { get; set; } = string.Empty;   // A201
        public string Building { get; set; } = string.Empty; // Tòa A
        public int Capacity { get; set; }
        public string RoomType { get; set; } = "Lý thuyết";  // Lý thuyết / Thực hành
        public bool IsAvailable { get; set; } = true;
    }
}
