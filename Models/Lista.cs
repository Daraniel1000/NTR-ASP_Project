using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace lab1.Models
{

    public class Lista
    {
        public Lista()
        {
            string jsonString = File.ReadAllText("data.json");
            data = JsonSerializer.Deserialize<Data>(jsonString);
            listy = new Listy();
            what = "1";
        }

        public static string[] Dni = { "Poniedziałek", "Wtorek", "Środa", "Czwartek", "Piątek" };
        public static string[] Godziny = {"8:00-8:45", "8:55-9:40", "9:50-11:35", "11:55:12:40", "12:50:13:35", "13:45:14:30","14:40-15:25",
        "15:35-16:20", "16:30-17:15"};

        public Listy listy;

        public Zajecia selectedSlot { get; set; }

        public int selectedID { get; set; }
        public string slotid { get; set; }

        public string what { get; set; }

        public string todelete { get; set; }

        public Data data { get; set; }

        public string selectedItem { get; set; }

        public string tytul
        {
            get
            {
                switch (Convert.ToInt32(what))
                {
                    case 1:
                        return "Zajęcia wybranej klasy";
                    case 2:
                        return "Zajęcia w wybranej sali";
                    case 3:
                        return "Zajęcia wybranego nauczyciela";
                }
                return "";
            }
        }

        public SelectList getSelectList()
        {
            switch (Convert.ToInt32(what))
            {
                case 1:
                    return new SelectList(data.groups);
                case 2:
                    return new SelectList(data.rooms);
                case 3:
                    return new SelectList(data.teachers);
            }
            return null;
        }

        public string getSlotRooms(int slot)
        {
            if (selectedItem == null) selectedItem = data.rooms[0];
            if (!data.isSlotOccupied(slot, selectedItem)) return " ";
            Zajecia zajecia = data.getSlot(slot, selectedItem);
            if (zajecia.room != selectedItem) return " ";
            return (zajecia.group);
        }

        public string getSlotGroups(int slot)
        {
            if (selectedItem == null) selectedItem = data.groups[0];
            if (!data.isSlotOccupied(slot, "", selectedItem, "")) return " ";
            Zajecia zajecia = data.getSlot(slot, "", selectedItem, "");
            if (zajecia.group != selectedItem) return " ";
            return (zajecia.room + " " + zajecia.@class);
        }

        public string getSlotTeachers(int slot)
        {
            if (selectedItem == null) selectedItem = data.teachers[0];
            if (!data.isSlotOccupied(slot, "", "", selectedItem)) return " ";
            Zajecia zajecia = data.getSlot(slot, "", "", selectedItem);
            if (zajecia.teacher != selectedItem) return " ";
            return (zajecia.room + " " + zajecia.@class + " " + zajecia.group);
        }

        public void changeSlot(Zajecia toChange)
        {
            if (data.isSlotOccupied(toChange)) data.getSlot(toChange).Change(toChange);
            else data.activities.Add(toChange);
            saveData();
        }

        public void saveData()
        {
            string jsonString = JsonSerializer.Serialize<Data>(data);
            File.WriteAllText("data.json", jsonString);
        }

        public void deleteSlot(Zajecia toDelete)
        {
            data.emptySlot(toDelete);
            saveData();
        }

    }

    public class Listy
    {
        public Listy()
        {
            rooms = new List<string>();
            groups = new List<string>();
            teachers = new List<string>();
        }
        public List<string> rooms { get; set; }
        public List<string> groups { get; set; }
        public List<string> teachers { get; set; }
    }

    public class Zajecia : IEquatable<Zajecia>
    {
        public string room { get; set; }
        public string group { get; set; }
        public string @class { get; set; }
        public int slot { get; set; }
        public string teacher { get; set; }

        public bool Equals(Zajecia other)
        {
            if (other == null) return false;
            return (this.slot == other.slot && (this.room == other.room || this.group == other.group || this.teacher == other.teacher));
        }

        public void Change(Zajecia other)
        {
            room = other.room;
            group = other.group;
            @class = other.@class;
            teacher = other.teacher;
            slot = other.slot;
        }
    }

    public class Data
    {
        public List<string> rooms { get; set; }
        public List<string> groups { get; set; }
        public List<string> classes { get; set; }
        public List<string> teachers { get; set; }
        public List<Zajecia> activities { get; set; }

        public bool isSlotOccupied(Zajecia other)
        {
            return activities.Contains(other);
        }

        public bool isSlotOccupied(int slot, string iroom = "", string igroup = "", string iteacher = "")
        {
            return activities.Contains(new Zajecia() { slot = slot, room = iroom, group = igroup, teacher = iteacher });
        }

        public Zajecia getSlot(Zajecia other)
        {
            if (isSlotOccupied(other)) return activities.Find(x => x.Equals(other));
            return null;
        }

        public Zajecia getSlot(int slot, string iroom = "", string igroup = "", string iteacher = "")
        {
            if (isSlotOccupied(slot, iroom, igroup, iteacher)) return activities.Find(x => x.Equals(new Zajecia() { slot = slot, room = iroom, group = igroup, teacher = iteacher }));
            return null;
        }

        public void emptySlot(Zajecia other)
        {
            if (isSlotOccupied(other)) activities.Remove(getSlot(other));
        }

        public void emptySlot(int slot, string iroom = "", string igroup = "", string iteacher = "")
        {
            if (isSlotOccupied(slot, iroom, igroup, iteacher)) activities.Remove(getSlot(slot, iroom, igroup, iteacher));
        }
    }

}