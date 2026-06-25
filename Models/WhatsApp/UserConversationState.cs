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

        private Dictionary<string, string> _tempData;
        [NotMapped]
        public Dictionary<string, string> TempData
        {
            get
            {
                if (_tempData == null)
                {
                    _tempData = string.IsNullOrEmpty(TempDataJson)
                        ? new Dictionary<string, string>()
                        : JsonSerializer.Deserialize<Dictionary<string, string>>(TempDataJson);
                }
                return _tempData;
            }
            set
            {
                _tempData = value;
                TempDataJson = JsonSerializer.Serialize(value);
            }
        }

        private Dictionary<int, ProjectPhotoInfo> _projectPhotoCount;
        [NotMapped]
        public Dictionary<int, ProjectPhotoInfo> ProjectPhotoCount
        {
            get
            {
                if (_projectPhotoCount == null)
                {
                    _projectPhotoCount = string.IsNullOrEmpty(ProjectPhotoInfoJson)
                        ? new Dictionary<int, ProjectPhotoInfo>()
                        : JsonSerializer.Deserialize<Dictionary<int, ProjectPhotoInfo>>(ProjectPhotoInfoJson);
                }
                return _projectPhotoCount;
            }
            set
            {
                _projectPhotoCount = value;
                ProjectPhotoInfoJson = JsonSerializer.Serialize(value);
            }
        }

        public void SyncJson()
        {
            if (_tempData != null) TempDataJson = JsonSerializer.Serialize(_tempData);
            if (_projectPhotoCount != null) ProjectPhotoInfoJson = JsonSerializer.Serialize(_projectPhotoCount);
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

