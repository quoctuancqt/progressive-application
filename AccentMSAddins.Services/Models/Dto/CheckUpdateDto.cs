namespace AccentMSAddins.Services.Models
{
    public class CheckUpdateDto
    {
        public ServerInfo ServerInfo { get; set; }
        public string FileId { get; set; }
        public string FileName { get; set; }
        public string FileUrl { get; set; }
        public bool AskFirst { get; set; }
        public bool IsConfirm { get; set; }
        public bool IsOverwrite { get; set; }
        public bool IsApplyAll { get; set; }
    }
}
