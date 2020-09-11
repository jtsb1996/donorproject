using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DonorProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonorProject.Controllers
{
    
    public class DonorController : Controller
    {
        private readonly ApplicationDBContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DonorController(UserManager<IdentityUser> userManager,
                               ApplicationDBContext db)
        {
            _db = db;
            _userManager = userManager;
        }
        [BindProperty]
        public DonationRecord donationRecord { get; set; }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ViewDonationRecord()
        {
            return View();
        }

        public IActionResult ViewReceipt(int id)
        {
            donationRecord = new DonationRecord();
            donationRecord = _db.donationRecords.FirstOrDefault(u => u.dID == id);
            if (donationRecord == null)
            {
                return NotFound();
            }
            return View(donationRecord);
        }

        public IActionResult CreateEditDonationRecord(int? id)
        {
            donationRecord = new DonationRecord();
            if (id == null)
            {
                //Create New Record
                return View(donationRecord);
            }
            //Update by retriving infomation from DB
            donationRecord = _db.donationRecords.FirstOrDefault(u => u.dID == id);
            if (donationRecord == null)
            {
                return NotFound();
            }

            return View(donationRecord);
        }

        [HttpPost]
        public IActionResult CreateEditDonationRecord()
        {
            if(ModelState.IsValid)
            {
                if (donationRecord.dID == 0)
                {
                    //Create
                    donationRecord.doneBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _db.donationRecords.Add(donationRecord);
                }
                else
                {
                    //Update
                    _db.donationRecords.Update(donationRecord);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            else
            {
                return View(donationRecord);
            }
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(new { data = await _db.donationRecords.ToListAsync() });
        }

        #endregion
    }
}
