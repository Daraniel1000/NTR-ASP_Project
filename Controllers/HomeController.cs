using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using lab1.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            getSlotGroups(lista);
            return View("Index", lista);
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
            lista.selectedSlot = new Zajecia()
            {
                slot = lista.selectedID,
                room = iroom,
                group = igroup,
                @class = lista.data.classes[0],
                teacher = iteacher
            };
            return View("SelectSlot", lista);
        }

        [HttpPost]
        public IActionResult ChangeSlot(Lista lista)
        {
            lista.selectedSlot.slot = Convert.ToInt32(lista.slotid);
            if (lista.data.isSlotOccupied(lista.selectedSlot)) throw new Exception("Slot already occupied");
            lista.changeSlot(lista.selectedSlot);
            getSlotGroups(lista);
            return View("Index", lista);
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
