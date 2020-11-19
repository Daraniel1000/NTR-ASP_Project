using System.ComponentModel.DataAnnotations;

namespace NTR20Z
{
    public class Teacher
    {
        public int TeacherID { get; set; }
        public string name { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class Subject
    {
        public int SubjectID { get; set; }
        public string name { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class Slot
    {
        public int SlotID { get; set; }
        public string name { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class Group
    {
        public int GroupID { get; set; }
        public string name { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class Room
    {
        public int RoomID { get; set; }
        public string name { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }
    }

    public class Assignment
    {
        public int AssignmentID { get; set; }
        public int SubjectID { get; set; }
        public int GroupID { get; set; }
        public int TeacherID { get; set; }
        public string? Comment { get; set; }
        [Timestamp]
        public byte[]? Timestamp { get; set; }

        public virtual Subject Subject { get; set; }
        public virtual Group Group { get; set; }
        public virtual Teacher Teacher { get; set; }
    }

    public class Activity
    {
        public int ActivityID {get;set;}
        public int SubjectID { get; set; }
        public int GroupID { get; set; }
        public int RoomID{get;set;}
        public int SlotID{get;set;}
        [Timestamp]
        public byte[]? Timestamp { get; set; }

        public virtual Subject Subject { get; set; }
        public virtual Group Group { get; set; }
        public virtual Room Room { get; set; }
        public virtual Slot Slot { get; set; }
    }
}