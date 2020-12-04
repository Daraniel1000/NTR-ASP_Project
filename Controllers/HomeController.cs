using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using lab1.Models;
using System.Text.Json;
using NTR20Z;

namespace lab1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult DeleteSlot(Lista lista)
        {
            if (lista.todelete == "1")
            {
                lista.selectedID = Convert.ToInt32(lista.slotid);
                string iroom = "", iteacher = "", igroup = "";
                switch (Convert.ToInt32(lista.what))
                {
                    case 1:
                        igroup = lista.selectedItem;
                        break;
                    case 2:
                        iroom = lista.selectedItem;
                        break;
                    case 3:
                        iteacher = lista.selectedItem;
                        break;
                }
                lista.selectedSlot = lista.data.getSlot(lista.selectedID, iroom, igroup, iteacher);
                lista.deleteSlot(lista.selectedSlot);
            }
            Lista fresh = new Lista() { what = lista.what, selectedItem = lista.selectedItem };
            getSlotGroups(fresh);
            return View("Index", fresh);
        }

        [HttpPost]
        public IActionResult SelectSlot(Lista lista)
        {
            lista.selectedID = Convert.ToInt32(lista.slotid);
            string iroom = "", iteacher = "", igroup = "";
            switch (Convert.ToInt32(lista.what))
            {
                case 1:
                    igroup = lista.selectedItem;
                    break;
                case 2:
                    iroom = lista.selectedItem;
                    break;
                case 3:
                    iteacher = lista.selectedItem;
                    break;
            }
            lista.selectedSlot = lista.data.getSlot(lista.selectedID, iroom, igroup, iteacher);
            if (lista.selectedSlot != null)
            {
                return Modify(lista);
            }
            lista.listy = new Listy();
            foreach (string it in lista.data.teachers)
            {
                if (!lista.data.isSlotOccupied(lista.selectedID, "", "", it))
                    lista.listy.teachers.Add(it);
            }
            foreach (string it in lista.data.groups)
            {
                if (!lista.data.isSlotOccupied(lista.selectedID, "", it))
                    lista.listy.groups.Add(it);
            }
            foreach (string it in lista.data.rooms)
            {
                if (!lista.data.isSlotOccupied(lista.selectedID, it))
                    lista.listy.rooms.Add(it);
            }
            if (lista.selectedSlot == null)
            {
                lista.selectedSlot = new Zajecia()
                {
                    slot = lista.selectedID,
                    room = iroom,
                    group = igroup,
                    @class = lista.data.classes[0],
                    teacher = iteacher
                };
            }
            return View("SelectSlot", lista);
        }

        public IActionResult Modify(Lista lista)
        {
            Modify model = new Modify()
            {
                selectedItem = lista.selectedItem,
                what = lista.what,
                listy = new Listy()
            };
            using (var db = new MyContext())
            {
                NTR20Z.Activity toModify = db.activities.Find(lista.selectedSlot.activityID);
                model.toModify = JsonSerializer.Serialize(toModify);
                model.group = db.groups.Find(toModify.GroupID).name;
                model.room = db.rooms.Find(toModify.RoomID).name;
                model.teacher = db.teachers.Find(db.assignments.Single(ass => ass.GroupID == toModify.GroupID &&
                ass.SubjectID == toModify.SubjectID).TeacherID).name;
                var act = db.activities.Join(db.assignments, activity => new Tuple<int, int>(activity.GroupID, activity.SubjectID),
                assignment => new Tuple<int, int>(assignment.GroupID, assignment.SubjectID), (activity, assignment) => new
                {
                    ActivityID = activity.ActivityID,
                    SlotID = activity.SlotID,
                    GroupID = activity.GroupID,
                    RoomID = activity.RoomID,
                    TeacherID = assignment.TeacherID,
                    SubjectID = assignment.SubjectID
                }).ToList();
                foreach (var teacher in db.teachers)
                {
                    if (!act.Any(act => act.TeacherID == teacher.TeacherID && act.SlotID == toModify.SlotID && act.ActivityID != toModify.ActivityID))
                    {
                        model.listy.teachers.Add(teacher.name);
                    }
                }
                foreach (var group in db.groups)
                {
                    if (!act.Any(act => act.GroupID == group.GroupID && act.SlotID == toModify.SlotID && act.ActivityID != toModify.ActivityID))
                    {
                        model.listy.groups.Add(group.name);
                    }
                }
                foreach (var room in db.rooms)
                {
                    if (!act.Any(act => act.RoomID == room.RoomID && act.SlotID == toModify.SlotID && act.ActivityID != toModify.ActivityID))
                    {
                        model.listy.rooms.Add(room.name);
                    }
                }
            }
            return View("Modify", model);
        }

        [HttpPost]
        public IActionResult Delete(Modify model)
        {
            NTR20Z.Activity toModify = JsonSerializer.Deserialize<NTR20Z.Activity>(model.toModify);
            using (var db = new MyContext())
            {
                NTR20Z.Activity toModifyFromDB = db.activities.Find(toModify.ActivityID);
                if (toModifyFromDB == null)
                {
                    return View("DeleteError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                if (toModifyFromDB.Timestamp != toModify.Timestamp)
                {
                    return View("DeleteError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                db.activities.Remove(toModifyFromDB);
                try
                {
                    Lista.mutex.WaitOne();
                    db.SaveChanges();
                    Lista.mutex.ReleaseMutex();
                }
                catch (Exception)
                {
                    Lista.mutex.ReleaseMutex();
                    return View("DeleteError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
            }
            return Index(new Lista() { what = model.what, selectedItem = model.selectedItem });

        }

        [HttpPost]
        public IActionResult Modify(Modify model)
        {
            NTR20Z.Activity toModify = JsonSerializer.Deserialize<NTR20Z.Activity>(model.toModify);
            using (var db = new MyContext())
            {
                NTR20Z.Activity toModifyFromDB = db.activities.Find(toModify.ActivityID);
                if (toModifyFromDB == null)
                {
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                if (toModifyFromDB.Timestamp != toModify.Timestamp)
                {
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                toModifyFromDB.GroupID = db.groups.Single(gr => gr.name == model.group).GroupID;
                toModifyFromDB.RoomID = db.rooms.Single(gr => gr.name == model.room).RoomID;
                int teacherID = db.teachers.Single(te => te.name == model.teacher).TeacherID;
                if(!db.assignments.Any(ass=>ass.GroupID==toModifyFromDB.GroupID && ass.TeacherID == teacherID))
                {
                    model.toModify = JsonSerializer.Serialize(toModifyFromDB);
                    ViewBag.subjectList = new List<string>();                    
                    var subjects = db.subjects.ToList();
                    bool isEmpty = true;
                    foreach (var it in subjects)
                    {
                        if (!db.assignments.Any(ass => ass.GroupID == toModifyFromDB.GroupID && ass.SubjectID == it.SubjectID))
                        {
                            ViewBag.subjectList.Add(it.name); 
                            isEmpty = false;
                        }
                    }
                    if(isEmpty)
                        return View("TeacherUnassignable", new Err(){what = model.what, selectedItem = model.selectedItem});
                    return View("SetAssignmentFromModify", model);
                }
                toModifyFromDB.SubjectID = db.assignments.Single(ass => ass.TeacherID == teacherID && ass.GroupID == toModifyFromDB.GroupID).SubjectID;
                try
                {
                    Lista.mutex.WaitOne();
                    db.SaveChanges();
                    Lista.mutex.ReleaseMutex();
                }
                catch (Exception)
                {
                    Lista.mutex.ReleaseMutex();
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
            }
            return Index(new Lista() { what = model.what, selectedItem = model.selectedItem });
        }

        [HttpPost]
        public IActionResult SetAssignmentFromModify(Modify model)
        {
            NTR20Z.Activity toModify = JsonSerializer.Deserialize<NTR20Z.Activity>(model.toModify);
            using (var db = new MyContext())
            {
                int teacher = db.teachers.Single(teacher=>teacher.name == model.teacher).TeacherID;
                int subject = db.subjects.Single(sub=>sub.name == model.subject).SubjectID;
                Lista.mutex.WaitOne();
                if (db.assignments.Any(ass => ass.GroupID == toModify.GroupID && (ass.TeacherID == teacher || ass.SubjectID == subject)))
                {
                    Lista.mutex.ReleaseMutex();
                    return View("TeacherUnassignable", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                db.assignments.Add(new Assignment()
                {
                    TeacherID = teacher,
                    GroupID = toModify.GroupID,
                    SubjectID = subject
                });
                db.SaveChanges();
                Lista.mutex.ReleaseMutex();
                NTR20Z.Activity toModifyFromDB = db.activities.Find(toModify.ActivityID);                
                if (toModifyFromDB == null)
                {
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                if (toModifyFromDB.Timestamp != toModify.Timestamp)
                {
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
                toModifyFromDB.GroupID = toModify.GroupID;
                toModifyFromDB.RoomID = toModify.RoomID;
                toModifyFromDB.SubjectID = subject;
                try
                {
                    Lista.mutex.WaitOne();
                    db.SaveChanges();
                    Lista.mutex.ReleaseMutex();
                }
                catch (Exception)
                {
                    Lista.mutex.ReleaseMutex();
                    return View("UpdateError", new Err(){what = model.what, selectedItem = model.selectedItem});
                }
            }            
            return Index(new Lista() { what = model.what, selectedItem = model.selectedItem });
        }

        [HttpPost]
        public IActionResult ChangeSlot(Lista lista)
        {
            lista.selectedSlot.slot = Convert.ToInt32(lista.slotid);
            ZajeciaDB zaj = new ZajeciaDB(lista.selectedSlot);
            using (var db = new MyContext())
            {
                if (!db.assignments.Any(ass => ass.TeacherID == zaj.teacher && ass.GroupID == zaj.group))
                {
                    lista.jsonString = System.Text.Json.JsonSerializer.Serialize(lista.selectedSlot);
                    lista.listy = new Listy();
                    var subjects = db.subjects.ToList();
                    foreach (var it in subjects)
                    {
                        if (!db.assignments.Any(ass => ass.GroupID == zaj.group && ass.SubjectID == it.SubjectID))
                        {
                            lista.listy.groups.Add(it.name);
                        }
                    }
                    if (!lista.listy.groups.Any()) return View("TeacherUnassignable", new Err(){what = lista.what, selectedItem = lista.selectedItem});
                    return View("TeacherError", lista);
                }
                zaj.subject = db.assignments.Single(ass => ass.GroupID == zaj.group && ass.TeacherID == zaj.teacher).SubjectID;
                lista.selectedSlot.@class = db.subjects.Find(zaj.subject).name;
            }
            try
            {
                lista.changeSlot(lista.selectedSlot);
            }
            catch (Exception e)
            {
                if (e.Message[0] != '1') throw e;
                return View("UpdateError", new Err(){what = lista.what, selectedItem = lista.selectedItem});
            }

            getSlotGroups(lista);
            return View("Index", lista);
        }

        [HttpPost]
        public IActionResult SetAssignment(Lista lista)
        {
            ZajeciaDB zaj = new ZajeciaDB(lista.selectedSlot);
            zaj.slot = Convert.ToInt32(lista.slotid);
            lista.selectedSlot = System.Text.Json.JsonSerializer.Deserialize<Zajecia>(lista.jsonString);
            using (var db = new MyContext())
            {
                Lista.mutex.WaitOne();
                if (db.assignments.Any(ass => ass.GroupID == zaj.group && (ass.TeacherID == zaj.teacher || ass.SubjectID == zaj.subject)))
                {
                    Lista.mutex.ReleaseMutex();
                    return View("TeacherUnassignable", new Err(){what = lista.what, selectedItem = lista.selectedItem});
                }
                db.assignments.Add(new Assignment()
                {
                    TeacherID = zaj.teacher,
                    GroupID = zaj.group,
                    SubjectID = zaj.subject
                });
                db.SaveChanges();
                Lista.mutex.ReleaseMutex();
                lista.selectedSlot.@class = db.subjects.Find(zaj.subject).name;
            }
            try
            {
                lista.changeSlot(lista.selectedSlot);
            }
            catch (Exception e)
            {
                if (e.Message[0] != '1') throw e;
                return View("UpdateError", new Err(){what = lista.what, selectedItem = lista.selectedItem});
            }
            Lista model = new Lista() { what = lista.what, selectedItem = lista.selectedItem };
            getSlotGroups(model);
            return View("Index", model);
        }

        public void getSlotGroups(Lista lista)
        {
            ViewBag.Slot = new string[41];
            int what = Convert.ToInt32(lista.what);
            for (int i = 1; i < 41; ++i)
            {
                switch (what)
                {
                    case 1:
                        ViewBag.Slot[i] = lista.getSlotGroups(i);
                        break;
                    case 2:
                        ViewBag.Slot[i] = lista.getSlotRooms(i);
                        break;
                    case 3:
                        ViewBag.Slot[i] = lista.getSlotTeachers(i);
                        break;
                }

            }
        }

        public IActionResult Index()
        {
            Lista lista = new Lista();
            getSlotGroups(lista);
            return View("Index", lista);
        }

        [HttpPost]
        public IActionResult Index(Lista lista)
        {
            if (ModelState.IsValid)
            {
                getSlotGroups(lista);
            }
            return View("Index", lista);
        }

        public IActionResult Rooms()
        {
            Lista lista = new Lista() { what = "2" };
            getSlotGroups(lista);
            return View("Index", lista);
        }

        public IActionResult Teachers()
        {
            Lista lista = new Lista() { what = "3" };
            getSlotGroups(lista);
            return View("Index", lista);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
