using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ComicVineApi;
using ComicVineApi.Models;

namespace Front_End.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index()
        {
            var client = new ComicVineClient("0a567d73ea058f0c705ca7d6fa13632a829573a8", "FalaGalera21");
            var Issues = client.Issue.Filter().OrderByDescending(p => p.DateAdded).Take(20).ToListAsync().Result;
            foreach(var issue in Issues)
            {
                if(issue.Name == null)
                {
                    var name = issue.Volume.Name;
                    issue.Name = name;
  
                }
            }
            return View(Issues);
        }
        public ActionResult Details(int id)
        {
            var client = new ComicVineClient("0a567d73ea058f0c705ca7d6fa13632a829573a8", "FalaGalera21");
            var Issue = client.Issue.GetAsync(id).Result;
            if(Issue.Name == null)
            {
                var name = Issue.Volume.Name;
                Issue.Name = name;
            }
            return View(Issue);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
