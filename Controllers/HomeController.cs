using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using lab1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            //i = Request.Form["slotid"];
            //Lista lista = new Lista();
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
                return View("Delete", lista);
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
                    if (!lista.listy.groups.Any()) return View("TeacherUnassignable");
                    return View("TeacherError", lista);
                }
                zaj.subject = db.assignments.Single(ass => ass.GroupID == zaj.group && ass.TeacherID == zaj.teacher).SubjectID;
                lista.selectedSlot.@class = db.subjects.Find(zaj.subject).name;
            }
            if (lista.data.isSlotOccupied(lista.selectedSlot)) throw new Exception("Slot already occupied");
            if (!lista.changeSlot(lista.selectedSlot))
            {
                return View("TeacherError");
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
                db.assignments.Add(new Assignment()
                {
                    TeacherID = zaj.teacher,
                    GroupID = zaj.group,
                    SubjectID = zaj.subject
                });
                db.SaveChanges();
                lista.selectedSlot.@class = db.subjects.Find(zaj.subject).name;
            }
            lista.changeSlot(lista.selectedSlot);
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
