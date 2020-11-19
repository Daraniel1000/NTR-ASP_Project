using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace lab1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //populateDatabase();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void populateDatabase()
        {
            setSlots();
            setData();
            setActivitiesAndAssignments();
        }
        private static void setSlots()
        {
            using (var db = new NTR20Z.MyContext())
            {
                db.Database.EnsureCreated();
                if (db.slots.Any()) return;
                int i = 1;
                foreach (string dzień in lab1.Models.Lista.Dni)
                {
                    foreach (string godzina in lab1.Models.Lista.Godziny)
                    {
                        db.slots.Add(new NTR20Z.Slot() { name = dzień + " " + godzina, SlotID = i++ });
                    }
                }
                db.SaveChanges();
            }
        }
        private static void setData()
        {
            using (var db = new NTR20Z.MyContext())
            {
                if (db.teachers.Any()) return;
                string jsonString = File.ReadAllText("data.json");
                lab1.Models.Data data = JsonSerializer.Deserialize<lab1.Models.Data>(jsonString);
                int i = 1;
                foreach (var teacher in data.teachers)
                {
                    db.teachers.Add(new NTR20Z.Teacher() { name = teacher, TeacherID = i++ });
                }
                i = 1;
                foreach (var room in data.rooms)
                {
                    db.rooms.Add(new NTR20Z.Room() { name = room, RoomID = i++ });
                }
                i = 1;
                foreach (var group in data.groups)
                {
                    db.groups.Add(new NTR20Z.Group() { name = group, GroupID = i++ });
                }
                i = 1;
                foreach (var subject in data.classes)
                {
                    db.subjects.Add(new NTR20Z.Subject() { name = subject, SubjectID = i++ });
                }
                db.SaveChanges();
            }
        }
        private static void setActivitiesAndAssignments()
        {
            using (var db = new NTR20Z.MyContext())
            {
                if (db.activities.Any()) return;
                string jsonString = File.ReadAllText("data.json");
                lab1.Models.Data data = JsonSerializer.Deserialize<lab1.Models.Data>(jsonString);
                foreach (var activity in data.activities)
                {
                    if (activity.slot == 0) continue;
                    NTR20Z.Teacher teach = db.teachers.Single(entity => entity.name == activity.teacher);
                    NTR20Z.Subject subj = db.subjects.Single(entity => entity.name == activity.@class);
                    NTR20Z.Group gr = db.groups.Single(entity => entity.name == activity.group);
                    NTR20Z.Room rm = db.rooms.Single(entity => entity.name == activity.room);
                    NTR20Z.Slot sl = db.slots.Find(activity.slot);
                    if(!db.assignments.Local.Any(assignment=>assignment.GroupID == gr.GroupID && assignment.TeacherID == teach.TeacherID))
                        db.assignments.Add(new NTR20Z.Assignment() { TeacherID = teach.TeacherID, SubjectID = subj.SubjectID, GroupID = gr.GroupID });
                    db.activities.Add(new NTR20Z.Activity() { SubjectID = subj.SubjectID, GroupID = gr.GroupID, RoomID = rm.RoomID, SlotID = sl.SlotID });
                }
                db.SaveChanges();
            }

        }
    }
}
