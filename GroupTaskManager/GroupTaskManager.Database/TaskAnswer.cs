using System.ComponentModel.DataAnnotations;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class TaskAnswer
    {
        [Key]
        public int Id { get; set; }
        public int Id_Task { get; set; }
        public string Id_User { get; set; }
        public string State { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedTime { get; set; }

        [StringLength(1000, ErrorMessage = "Result cannot exceed 1000 characters.")]
        public string? ResultString { get; set; }
        public byte[]? ResultFile { get; set; }


        public TaskRecord TaskRecord { get; set; }
    }
}
