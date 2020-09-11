using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DonorProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace DonorProject.Controllers
{
    [Authorize]
    public class StaffController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDBContext _db;

        public StaffController(UserManager<IdentityUser> userManager,
                                SignInManager<IdentityUser> signInManager, 
                                ApplicationDBContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        public DonorRecord donorRecord { get; set; }
        public DonationRecord donationRecord { get; set; }

        public IActionResult Index()
        {
            return View();
        }
        
        public IActionResult ViewDonationRecord()
        {
            return View();
        }

        public IActionResult ViewDonorRecord()
        {
            return View();
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
        public IActionResult CreateEditDonationRecord(DonationRecord donationRecord)
        {
            if (ModelState.IsValid)
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
        

        public IActionResult CreateEditDonorRecord(string? id)
        {
            donorRecord = new DonorRecord();
            if (id == null)
            {
                //Create New Record
                return View(donorRecord);
            }
            //Update by retriving infomation from DB
            donorRecord = _db.donorRecords.FirstOrDefault(u => u.dID == id);
            if (donorRecord == null)
            {
                return NotFound();
            }

            return View(donorRecord);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEditDonorRecord(DonorRecord donorRecord)
        {
            if (ModelState.IsValid)
            {
                if (donorRecord.dID == null)
                {
                    //Create an Identity for the new Donor, based on the generated ID, assign the ID to the donor.dID
                    var user = new IdentityUser { UserName = donorRecord.username, Email = donorRecord.username };
                    var result = await _userManager.CreateAsync(user, donorRecord.password);
                    if (result.Succeeded)
                    {
                        donorRecord.dID = user.Id;
                        await _userManager.AddToRoleAsync(user, "Donor");
                        _db.donorRecords.Add(donorRecord);
                    }
                    
                }
                else
                {
                    //Update
                    _db.donorRecords.Update(donorRecord);
                    var user = await _userManager.FindByIdAsync(donorRecord.dID);
                    var result = await _userManager.SetUserNameAsync(user, donorRecord.username);
                    await _userManager.SetEmailAsync(user, donorRecord.username);
                    await _userManager.ChangePasswordAsync(user, user.PasswordHash, donorRecord.password);
                }
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            else
            {
                return View(donorRecord);
            }
        }

        #region API Calls
        [HttpGet]
        public async Task<IActionResult> GetDonorRecord()
        {
            return Json(new { data = await _db.donorRecords.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDonorRecord(string id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var donorRecordDB = await _db.donorRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (donorRecordDB == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            if (_db.donationRecords.Any(u => u.doneBy == id) == true)
            {
                return Json(new { success = false, message = "Donor has Donation Record" });
            }
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
            _db.donorRecords.Remove(donorRecordDB);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonationRecord()
        {
            return Json(new { data = await _db.donationRecords.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDonationRecord(int id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var donationRecordDB = await _db.donationRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (donationRecordDB == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            if (donationRecord.status == "Confirm")
            {
                return Json(new { success = false, message = "Donation has been confirmed" });
            }
            _db.donationRecords.Remove(donationRecordDB);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Delete Successful" });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmDonationRecord(int id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var donationRecordDB = await _db.donationRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (donationRecordDB == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            if (donationRecordDB.status == "Not Confirm")
            {
                donationRecordDB.status = "Confirm";
                _db.donationRecords.Update(donationRecordDB);
                await _db.SaveChangesAsync();

                return Json(new { success = true, message = "Confirm Successful" });
            }

            else
            {
                return Json(new { success = false, message = "Donation Status is already Confirm" });
            }
        }
        #endregion
    }
}
