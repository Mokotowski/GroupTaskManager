using System.ComponentModel.DataAnnotations;

namespace GroupTaskManager.GroupTaskManager.Database
{
    public class TaskRecord
    {
        [Key]
        public int Id { get; set; }
        public int Id_Group { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Answer type is required.")]
        [StringLength(50, ErrorMessage = "Answer type cannot exceed 50 characters.")]
        public string AnswerType { get; set; }

        [Required(ErrorMessage = "Add time is required.")]
        public DateTime AddTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public DateTime EndTime { get; set; }

        public Group Group { get; set; }
        public List<TaskAnswer> TaskAnswer { get; set; } = new List<TaskAnswer>();
    }
}
