namespace ProjectManagementWebApp.Models
{
    public class ProjectLecturer
    {
        public int ProjectId { get; set; }

        public virtual Project Project { get; set; }

        public string LecturerId { get; set; }

        public virtual Lecturer Lecturer { get; set; }
    }
}