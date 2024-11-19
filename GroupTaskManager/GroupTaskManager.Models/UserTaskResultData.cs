using GroupTaskManager.GroupTaskManager.Database;

namespace GroupTaskManager.GroupTaskManager.Models
{
    public class UserTaskResultData
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }


        public int Id_TaskAnswer { get; set; }
        public int Id_TaskRecord { get; set; }
        public string Id_User { get; set; }
        public string State { get; set; }
        public bool Completed { get; set; }
        public DateTime? CompletedTime { get; set; }
        public string? ResultString { get; set; }
        public byte[]? ResultFile { get; set; }
        public string? ResultFileExtension { get; set; }
        public string? ResultComment { get; set; }


        public int Id_Group { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string AnswerType { get; set; }
        public DateTime AddTime { get; set; }
        public DateTime EndTime { get; set; }

        public UserTaskResultData() { }

        public UserTaskResultData(TaskAnswer answer, TaskRecord record, UserModel user)
        {
            this.Firstname = user.Firstname;
            this.Lastname = user.Lastname;
            this.Id_TaskAnswer = answer.Id;
            this.Id_TaskRecord = record.Id;
            this.Id_User = answer.Id_User;
            this.State = answer.State;
            this.Completed = answer.Completed;
            this.CompletedTime = answer.CompletedTime;
            this.ResultString = answer.ResultString;
            this.ResultFile = answer.ResultFile;
            this.ResultFileExtension = answer.ResultFileExtension;
            this.ResultComment = answer.ResultComment;
            this.Id_Group = record.Id_Group;
            this.Name = record.Name;
            this.Description = record.Description;
            this.AnswerType = record.AnswerType;
            this.AddTime = record.AddTime;
            this.EndTime = record.EndTime;
        }
    }
}
