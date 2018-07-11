using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IEPprojekat.Models;

namespace IEPprojekat.Controllers



{
    [AdminAuthorize(Roles = "Administrator")]
    public class InformationsForAdministratorsController : Controller
    {
        private Auctions db = new Auctions();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(InformationsForAdministratorsController));


        // GET: InformationsForAdministrators/Edit/5
        public ActionResult Edit()
        {
            log.Info("GET action /InformationsForAdministrators/Edit has been fired.");

            InformationsForAdministrator informationsForAdministrator = db.InformationsForAdministrator.FirstOrDefault();
            if (informationsForAdministrator == null)
            {
                return View("ErrorPage");
            }
            return View(informationsForAdministrator);
        }

        // POST: InformationsForAdministrators/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ItemsPerPage,SilverPack,GoldPack,PlatinumPack,ValueToken,Currency")] InformationsForAdministrator informationsForAdministrator)
        {
            log.Info("POST action /InformationsForAdministrators/Edit has been fired.");

            if (ModelState.IsValid)
            {
                db.Entry(informationsForAdministrator).State = EntityState.Modified;
                db.SaveChanges();
                
            }
            return View(informationsForAdministrator);
        }

       

      

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
