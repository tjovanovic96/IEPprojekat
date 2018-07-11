using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IEPprojekat.Hubs;
using IEPprojekat.Models;
using IepWebApp;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using PagedList;

namespace IEPprojekat.Controllers
{
    public class OrdersController : Controller
    {
        private Auctions db = new Auctions();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(OrdersController));

        


        [System.Web.Mvc.Authorize(Roles = "Administrator, RegularUser")]
        public ActionResult Index(int? page)
        {
            log.Info("Action /Orders/Index has been fired.");

            var order = from Order in db.Order select Order;
            string id = User.Identity.GetUserId();
            order = order.Where(o => o.IdUser.Equals(id)).OrderBy(o => o.NumberOfTokens);
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            return View(order.ToPagedList(pageNumber, pageSize));
        }

        [AdminAuthorize(Roles = "Administrator")]
        public ActionResult IndexAdmin(int? page)
        {
            log.Info("Action /Orders/IndexAdmin has been fired.");

            var order = from Order in db.Order select Order;
            string id = User.Identity.GetUserId();
            order = order.Where(o => o.CurrentState.Equals("SUBMITTED")).OrderBy(o=>o.NumberOfTokens);
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            return View(order.ToPagedList(pageNumber, pageSize));
        }

        [System.Web.Mvc.Authorize(Roles = "Administrator, RegularUser")]
        public ActionResult OrderTokens()
        {
            log.Info("Action /Orders/OrderTokens has been fired.");

            decimal price = (decimal)db.InformationsForAdministrator.First().GoldPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.GoldPrice = price;
            ViewBag.GoldPack = db.InformationsForAdministrator.First().GoldPack;
            price = (decimal)db.InformationsForAdministrator.First().SilverPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.SilverPrice = price;
            ViewBag.SilverPack = db.InformationsForAdministrator.First().SilverPack;
            price = (decimal)db.InformationsForAdministrator.First().PlatinumPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.PlatinumPrice = price;
            ViewBag.PlatinumPack = db.InformationsForAdministrator.First().PlatinumPack;
            ViewBag.Currency = db.InformationsForAdministrator.First().Currency;

            return View();
        }

        

       

       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult CancelOrder(Guid reference)
        {
            log.Info("Action /Orders/OrderNow has been fired.");
            Order order = db.Order.Find(reference);
            if (order == null)
            {
                return View("ErrorPage");

            }
            if (!order.CurrentState.Equals("SUBMITTED"))
            {
                return View("ErrorPage");

            }
            order.CurrentState = "CANCELED";
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("OrderTokens");

        }

        [System.Web.Mvc.Authorize(Roles = "Administrator, RegularUser")]
        public ActionResult OrderNow(int? id)
        {
            log.Info("Action /Orders/OrderNow has been fired.");

            if (id == null || id > 3)
            {
                return View("ErrorPage");
            }

            int numberOfTokens;
            if (id == 1)
            {
                numberOfTokens = (int)db.InformationsForAdministrator.First().SilverPack;
            } else if (id == 2)
            {
                numberOfTokens = (int)db.InformationsForAdministrator.First().GoldPack;

            } else
            {
                numberOfTokens = (int)db.InformationsForAdministrator.First().PlatinumPack;
            }
            Order order = new Order();
            order.IdOrder = Guid.NewGuid();
            order.IdUser = User.Identity.GetUserId();
            order.NumberOfTokens = numberOfTokens;
            order.RealPrice = numberOfTokens * db.InformationsForAdministrator.First().ValueToken;
            order.CurrentState = "SUBMITTED";
            db.Order.Add(order);
            db.SaveChanges();
            decimal price = (decimal)db.InformationsForAdministrator.First().GoldPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.GoldPrice = price;
            ViewBag.GoldPack = db.InformationsForAdministrator.First().GoldPack;
            price = (decimal)db.InformationsForAdministrator.First().SilverPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.SilverPrice = price;
            ViewBag.SilverPack = db.InformationsForAdministrator.First().SilverPack;
            price = (decimal)db.InformationsForAdministrator.First().PlatinumPack * (decimal)db.InformationsForAdministrator.First().ValueToken;
            ViewBag.PlatinumPrice = price;
            ViewBag.PlatinumPack = db.InformationsForAdministrator.First().PlatinumPack;
            ViewBag.Currency = db.InformationsForAdministrator.First().Currency;
            ViewBag.Order = order.IdOrder;
            return View("Centili");
        }

        [AllowAnonymous]
        public ActionResult CentiliPayment(Guid clientid, string status)
        {
            log.Info("Action /Orders/CentiliPayment has been fired.");
            
            Order order = db.Order.Find(clientid);
            if (order == null)
            {
                return View("ErrorPage");
            }
            if (order.CurrentState != "SUBMITTED")
            {
                return View("OrderAlreadyProcessed");
            }
            if (status == "canceled" || status == "failed")
            {
                order.CurrentState = "CANCELED"; 
            }
            else
            {
                order.CurrentState = "COMPLETED"; 
                User user = db.User.Find(order.IdUser);
                if (user == null)
                {
                    return View("ErrorPage");
                }
                user.NumberOfTokens = (int)user.NumberOfTokens + (int)order.NumberOfTokens;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                Mailer.sendMail(user.Email, "Centili payment", "Your payment was successful.");


            }
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
            var hubC = GlobalHost.ConnectionManager.GetHubContext<TokenHub>();
            User u = db.User.Find(order.IdUser);
             hubC.Clients.All.tokens(null, null, order.IdUser, u.NumberOfTokens);


            return new HttpStatusCodeResult(200);

        }
    }
}
