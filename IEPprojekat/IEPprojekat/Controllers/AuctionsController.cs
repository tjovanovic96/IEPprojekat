using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using IEPprojekat.Hubs;
using IEPprojekat.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using PagedList;

namespace IEPprojekat.Controllers
{
    
    public class AuctionsController : Controller
    {
        private Auctions db = new Auctions();
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(AuctionsController));


        [AllowAnonymous]
        public ActionResult Index(int? page)
        {
            log.Info("Action /Auction/Index has been fired.");

            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            ViewBag.Page = "Index";
            return View(db.Auction.OrderByDescending(a => a.TimeOpening).ToPagedList(pageNumber, pageSize));
        }

        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult WonAuctions(int? page)
        {
            log.Info("Action /Auction/WonAuctions has been fired.");
            string id = User.Identity.GetUserId();
            var auctions = from Auction in db.Auction select Auction;
           
           auctions = auctions.Where(a => id.Equals(a.IdWinner));
            auctions = auctions.Where(a => a.State.Equals("COMPLETED"));
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            ViewBag.Page = "WonAuctions";
            return View("Index", auctions.OrderByDescending(m=>m.TimeClosing).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Search(string name, string minPrice, string maxPrice, string state, int? page)
        {
            log.Info("Action /Auction/Search has been fired.");
            if (String.IsNullOrEmpty(name) && String.IsNullOrEmpty(minPrice) && String.IsNullOrEmpty
                (maxPrice) && state == "")
            {
                return RedirectToAction("Index");
            }
            var auctions = from Auction in db.Auction select Auction;
                if (!String.IsNullOrEmpty(name))
                {
                auctions = auctions.Where(n => n.Name.Contains(name));
                }
                if (!String.IsNullOrEmpty(minPrice))
            {
                decimal min;
                if (Decimal.TryParse(minPrice, out min))
                {
                    auctions = auctions.Where(n => n.CurrentPrice >= min);
                    
                } else
                {
                    var a = from Auction in db.Auction select Auction;
                    ViewBag.ErrorMin = "Price must be a number";
                    int pageSize2 = (int)db.InformationsForAdministrator.First().ItemsPerPage;
                    int pageNumber2 = (page ?? 1);
                    ViewBag.Page = "Index";
                    return View("Index", a.OrderByDescending(m => m.TimeOpening).ToPagedList(pageNumber2, pageSize2));
                }
            }
            if (!String.IsNullOrEmpty(maxPrice))
            {
                decimal max;
                if (Decimal.TryParse(maxPrice, out max))
                {
                    auctions = auctions.Where(n => n.CurrentPrice <= max);
                    
                } else
                {
                    var a = from Auction in db.Auction select Auction;
                    ViewBag.ErrorMax = "Price must be a number";
                    int pageSize1 = (int)db.InformationsForAdministrator.First().ItemsPerPage;
                    int pageNumber1 = (page ?? 1);
                    ViewBag.Page = "Index";
                    return View("Index", a.OrderByDescending(m => m.TimeOpening).ToPagedList(pageNumber1, pageSize1));
                }
            }
            if (state != "" && state != null)
            {
                auctions = auctions.Where(n => n.State.Equals(state));
            }
            auctions = auctions.OrderByDescending(a => a.TimeOpening);
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            ViewBag.Page = "Search";
            ViewBag.AuctionName = name;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.State = state;
            return View("Index", auctions.ToPagedList(pageNumber, pageSize));

        }

        // GET: Auctions/Details/5
        public ActionResult Details(Guid? id)
        {
            log.Info("Action /Auction/Details has been fired.");
            if (id == null)
            {
                return View("ErrorPage");
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return RedirectToAction("Index");
            }
            return View(auction);
        }

        [AdminAuthorize(Roles = "Administrator")]
        public ActionResult ReadyAuctions(int? page)
        {
            log.Info("Action /Auction/ReadyAuctions has been fired.");
            var auctions = from Auction in db.Auction select Auction;
            auctions = auctions.Where(n => n.State.Equals("READY"));
            auctions = auctions.OrderByDescending(a => a.TimeOpening);
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            return View(auctions.ToPagedList(pageNumber, pageSize));
        }

        // GET: Auctions/Create
        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult Create()
        {
            log.Info("GET action /Auction/Create has been fired.");
            return View();
        }

        // POST: Auctions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult Create([Bind(Include = "IdAuction,Name,Image,Duration,StartingPrice")] Auction auction, HttpPostedFileBase file)
        {
            log.Info("POST action /Auction/Create has been fired.");
            if (ModelState.IsValid)
            {
                auction.IdAuction = Guid.NewGuid();
                auction.CurrentPrice = auction.StartingPrice;
                auction.State = "READY";
                auction.Created = DateTime.Now;
                auction.IdOwner = User.Identity.GetUserId();
                auction.IdWinner = null;
                if (file != null)
                {
                    using (System.IO.MemoryStream ms = new MemoryStream())
                    {
                        file.InputStream.CopyTo(ms);
                        auction.Image = ms.GetBuffer();
                    }
                }
                db.Auction.Add(auction);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(auction);
        }

        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult MyAuctions(int? page)
        {
            log.Info("Action /Auction/MyAuctions has been fired.");
            var auctions = from Auction in db.Auction select Auction;
            string userId = User.Identity.GetUserId();
            auctions = auctions.Where(n => n.IdOwner.Equals(userId));
            auctions = auctions.OrderByDescending(a => a.TimeOpening);
            int pageSize = (int)db.InformationsForAdministrator.First().ItemsPerPage;
            int pageNumber = (page ?? 1);
            ViewBag.Page = "MyAuctions";
            return View("Index", auctions.ToPagedList(pageNumber, pageSize));
        }

       

       

        // GET: Auctions/Delete/5
        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult Delete(Guid? id)

        {
            log.Info("GET action /Auction/Delete has been fired.");
            if (id == null)
            {
                return View("ErrorPage");
            }
            Auction auction = db.Auction.Find(id);
           
            if (auction == null)
            {
                return View("ErrorPage");
            }
            if (User.Identity.GetUserId() == auction.IdOwner)
            {
                return View(auction);
            } else
            {
                return RedirectToAction("Index");
            }
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [System.Web.Mvc.Authorize(Roles = "RegularUser, Administrator")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            log.Info("POST action /Auction/DeleteConfirmed has been fired.");
            Auction auction = db.Auction.Find(id);
            if (User.Identity.GetUserId() != auction.IdOwner)
            {
                return RedirectToAction("Index");
            }
           
           
            string winnerID = auction.IdWinner;
            User winner = db.User.Find(winnerID);
            if (winner != null && auction.State != "COMPLETED")
            {

                winner.NumberOfTokens += (int)auction.CurrentPrice;
                db.Entry(winner).State = EntityState.Modified;
                db.SaveChanges();
                var hubCon = GlobalHost.ConnectionManager.GetHubContext<TokenHub>();
                hubCon.Clients.All.tokens(winner.Id, winner.NumberOfTokens, null, null);
            }

            db.Auction.Remove(auction);
            db.SaveChanges();
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            hubContext.Clients.All.deleteauction(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [AdminAuthorize(Roles = "Administrator")]
        public ActionResult StartAuction(Guid? id)
        {
            log.Info("Action /Auction/StartAuction has been fired.");
            if (id == null)
            {
                return View("ErrorPage");
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return View("ErrorPage");
            }
            if (auction.State.Equals("OPENED") || auction.State.Equals("COMPLETED"))
            {
                return RedirectToAction("ReadyAuctions");
            }
            auction.State = "OPENED";
            auction.TimeOpening = DateTime.Now;
            auction.TimeClosing = DateTime.Now.AddSeconds((double)auction.Duration);
            db.Entry(auction).State = EntityState.Modified;
            db.SaveChanges();
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            hubContext.Clients.All.addauction();
            return RedirectToAction("ReadyAuctions");
        }

        [AllowAnonymous]
        [AjaxOnly]
        [HttpPost, ActionName("FinishAuction")]
        public void FinishAuction(Guid? id)
        {
            log.Info("POST action /Auction/FinishAuction has been fired.");
            var auction = db.Auction.Find(id);
            if (auction == null)
            {
                return;
            }
            if (auction.State == "COMPLETED" || auction.State == "READY")
            {
                return;
            }
            auction.State = "COMPLETED";
            db.Entry(auction).State = EntityState.Modified;
            db.SaveChanges();
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            var user = db.User.Find(auction.IdWinner);
            if (user != null)
            {
                hubContext.Clients.All.finished(auction.IdAuction, user.Email);
            }
            return;
        }

        [AllowAnonymous]
        [AjaxOnly]
        [HttpPost, ActionName("FinishAuctionDetails")]
        public void FinishAuctionDetails(Guid? id)
        {
            log.Info("POST action /Auction/FinishAuctionDetails has been fired.");
            var auction = db.Auction.Find(id);
            if (auction == null)
            {
                return;
            }
            if (auction.State == "COMPLETED" || auction.State == "READY")
            {
                return;
            }
            auction.State = "COMPLETED";
            db.Entry(auction).State = EntityState.Modified;
           
            db.SaveChanges();
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
            var user = db.User.Find(auction.IdWinner);
            if (user != null)
            {
                hubContext.Clients.All.finished(auction.IdAuction,user.Email);
            }

            return;
        }

        public void returnTokens(Auction auction)
        {
            if (auction.IdWinner == null) return;
            User user = db.User.Find(auction.IdWinner);
            if (user == null) return;
            user.NumberOfTokens += (int)auction.CurrentPrice;
        }

        public bool checkNumberOfTokens(Auction auction, User user)
        {
            if (((auction.IdWinner == null || auction.IdWinner != user.Id) && user.NumberOfTokens <= auction.CurrentPrice) || (auction.IdWinner != null && auction.IdWinner == user.Id && user.NumberOfTokens < 1))
            {
                return false;
            }
            return true;
        }

        public void updateAuctionAndBid(Auction auction)
        {
            auction.CurrentPrice++;
            auction.IdWinner = User.Identity.GetUserId();
            Bid bid = new Bid();
            bid.IdAuction = (Guid)auction.IdAuction;
            bid.IdUser = User.Identity.GetUserId();
            bid.TimeOfBidding = DateTime.Now;
            bid.TokensGiven = auction.CurrentPrice + 1;
            db.Bid.Add(bid);

        }

        public void reserveTokens(Auction auction)
        {
            User user = db.User.Find(User.Identity.GetUserId());
            user.NumberOfTokens -= (int)auction.CurrentPrice;
        }

        [System.Web.Mvc.Authorize(Roles ="Administrator, RegularUser")]
        public async Task<ActionResult> BidNow(Guid? id, byte[] version)
        {
            log.Info("Action /Auction/BidNow has been fired.");
           

            using (var trans = db.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    if (id == null || version == null)
                    {
                        throw new Exception();
                    }
                    Auction auction = db.Auction.Find(id);
                    if (auction == null)
                    {
                        throw new Exception();
                    }
                    if (auction.State.Equals("READY") || auction.State.Equals("COMPLETED"))
                    {
                        throw new Exception();
                    }
                    User lastUser = auction.Winner;
                    string idLastUser = auction.IdWinner;
                    string idUser = User.Identity.GetUserId();
                    User user = db.User.Find(idUser);
                    if (!checkNumberOfTokens(auction, user))
                    {
                        throw new Exception();
                    }
                    returnTokens(auction);
                    updateAuctionAndBid(auction);
                    reserveTokens(auction);
                    db.Entry(auction).OriginalValues["AuctionRowVersion"] = version;

                    await db.SaveChangesAsync();
                    var hubContext = GlobalHost.ConnectionManager.GetHubContext<AuctionHub>();
                    hubContext.Clients.All.refresh(auction.IdAuction, auction.CurrentPrice, db.User.Find(idUser).Email);
                    hubContext.Clients.All.bidding(db.User.Find(idUser).Email);
                    string v = @Convert.ToBase64String(auction.AuctionRowVersion);
                    hubContext.Clients.All.newversion(auction.IdAuction, v);
                    var hubC = GlobalHost.ConnectionManager.GetHubContext<TokenHub>();
                    if (lastUser == null)
                    {
                        hubC.Clients.All.tokens(null, null, user.Id, user.NumberOfTokens);
                    }
                    else
                    {
                        int number = db.User.Find(idLastUser).NumberOfTokens;
                        hubC.Clients.All.tokens(idLastUser, number, user.Id, user.NumberOfTokens);
                    }
                    trans.Commit();
                    return RedirectToAction("Index");

                } catch (Exception ex)
                {
                    trans.Rollback();
                    log.Error("Bid is not successful!");
                    return RedirectToAction("Index");
                }
                
            }
           
           
           
          
            
           
            
          
           
            
         

           
           

           

            
            
            
        }

        
    }
}


