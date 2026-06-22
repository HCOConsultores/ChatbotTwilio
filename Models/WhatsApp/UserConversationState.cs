using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Twilio.Types;

namespace chatBotTwilio.Models.WhatsApp
{
    public class ConversationState
    {

        public int Id { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CurrentState { get; set; }
        public string? PreviousState { get; set; }
        public string? CurrentContract { get; set; }
        public int? CurrentProjectId { get; set; }
        public string? CurrentProject { get; set; }
        public string? CurrentNoteType { get; set; }
        public string? Supervisor { get; set; }
        public string? Ot { get; set; }
        public string? Descripcorta { get; set; }
        public DateTime LastInteraction { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Nuevo campo para almacenar la información de fotos por proyecto
        public string? ProjectPhotoInfoJson { get; set; }

        // Nuevo campo para almacenar datos temporales durante los flujos de captura
        public string? TempDataJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> TempData
        {
            get
            {
                return string.IsNullOrEmpty(TempDataJson)
                    ? new Dictionary<string, string>()
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(TempDataJson);
            }
            set
            {
                TempDataJson = JsonSerializer.Serialize(value);
            }
        }

        [NotMapped]
        public Dictionary<int, ProjectPhotoInfo> ProjectPhotoCount
        {
            get
            {
                return string.IsNullOrEmpty(ProjectPhotoInfoJson)
                    ? new Dictionary<int, ProjectPhotoInfo>()
                    : JsonSerializer.Deserialize<Dictionary<int, ProjectPhotoInfo>>(ProjectPhotoInfoJson);
            }
            set
            {
                ProjectPhotoInfoJson = JsonSerializer.Serialize(value);
            }
        }

        public ConversationState(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
            CurrentState = "INICIO";
            LastInteraction = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
            ProjectPhotoCount = new Dictionary<int, ProjectPhotoInfo>();
            TempData = new Dictionary<string, string>();
        }

        public class ProjectPhotoInfo
        {
            public int PhotoCount { get; set; }
            public DateTime LastPhotoDate { get; set; }
        }

    }
}

