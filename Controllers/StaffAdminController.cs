using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using DonorProject.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace DonorProject.Controllers
{
    public class StaffAdminController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDBContext _db;

        public StaffAdminController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ApplicationDBContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }
        public UserAccounts userAccount { get; set; }
        public DonorRecord donorRecord { get; set; }
        public DonationRecord donationRecord { get; set; }
        public DonorRecordArchive donorRecordArchive { get; set; }
        public DonationRecordArchive donationRecordArchive { get; set; }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ViewUserAccount()
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

        public IActionResult ViewResetStatus()
        {
            return View();
        }
        public IActionResult ViewDonationRecordArchive()
        {
            return View();
        }

        public IActionResult ViewDonorRecordArchive()
        {
            return View();
        }

        // Create or Edit Donation Record
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

        // converts to csv 
        public IActionResult DonationConvertToCSV()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Name, Nric, Type, Address, Contactno, Receipt No, Date of Reciept, Dateofdonation, " +
                "Amount of donation, Mode of Donation, Purpose of donation, Remarks, Tax exemption, Status");
            foreach (var donationRecord in _db.donationRecords)
            {
                csvBuilder.AppendLine($"{donationRecord.name},{donationRecord.nric},{donationRecord.type},{donationRecord.address}," +
                    $"{donationRecord.contactNo},{donationRecord.receiptNo},{donationRecord.dateOfReceipt},{donationRecord.dateOfDonation},{donationRecord.amountOfDonation}," +
                    $"{donationRecord.modeOfDonation},{donationRecord.purposeOfDonation},{donationRecord.remarks},{donationRecord.taxExemption},{donationRecord.status}");
            }
            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", "donationRecord.csv");
        }

        // Create or Edit User Accounts
        public async Task<IActionResult> CreateEditUserAccount(string? id)
        {
            userAccount = new UserAccounts();
            if (id == null)
            {
                //Create New Record
                return View(userAccount);
            }
            //Update by retriving infomation from DB
            var user = await _userManager.FindByIdAsync(id);
            userAccount.Id = user.Id;
            userAccount.UserName = user.UserName;
            userAccount.Password = user.PasswordHash;
            if (userAccount == null)
            {
                return NotFound();
            }

            return View(userAccount);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEditUserAccount(UserAccounts userAccount)
        {
            if (ModelState.IsValid)
            {
                if (userAccount.Id == null)
                {
                    var user = new IdentityUser { UserName = userAccount.UserName, Email = userAccount.UserName };
                    var result = await _userManager.CreateAsync(user, userAccount.Password);
                    if (userAccount.TypeOfUser == "StaffAdmin")
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _userManager.AddToRoleAsync(user, "StaffAdmin");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        await _userManager.AddToRoleAsync(user, "Staff");
                        return RedirectToAction("Index");
                    }

                }
                else
                {
                    //Update
                    var user = await _userManager.FindByEmailAsync(userAccount.UserName);
                    await _userManager.SetUserNameAsync(user, userAccount.UserName);
                    await _userManager.SetEmailAsync(user, userAccount.UserName);
                    await _userManager.ChangePasswordAsync(user, user.PasswordHash, userAccount.Password);
                }
                return RedirectToAction("Index");
            }

            else
            {
                return View(userAccount);
            }
        }

        // Create or Edit Donor Record
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

        // converts to csv 
        public IActionResult DonorExportToCSV()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Name, Nric, UserName, Address, Contactno");
            foreach (var donorRecord in _db.donorRecords)
            {
                csvBuilder.AppendLine($"{donorRecord.name},{donorRecord.nric},{donorRecord.username},{donorRecord.address},{donorRecord.contactno}");
            }
            return File(Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", "donorRecord.csv");
        }
       
        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetUserRecord()
        {
            return Json(new { data = (await _userManager.Users.ToListAsync()).ToList() });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonorRecord()
        {
            return Json(new { data = await _db.donorRecords.ToListAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonationRecord()
        {
            return Json(new { data = await _db.donationRecords.ToListAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonationArchive()
        {
            return Json(new { data = await _db.donationRecordArchives.ToListAsync() });
        }

        [HttpGet]
        public async Task<IActionResult> GetDonorArchive()
        {
            return Json(new { data = await _db.donorRecordArchives.ToListAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUserAccount(string id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var user = await _userManager.FindByIdAsync(id);
            var donorRecordDB = await _db.donorRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (user == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            if (_db.donationRecords.Any(u => u.doneBy == id) == true)
            {
                return Json(new { success = false, message = "Donor has Donation Record" });
            }
            else
            {
                if (await _userManager.IsInRoleAsync(user, "Donor"))
                    _db.donorRecords.Remove(donorRecordDB);
            }
            await _userManager.DeleteAsync(user);
            await _db.SaveChangesAsync();

            return Json(new { success = true, message = "Delete Successful" });
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

        [HttpDelete]
        public async Task<IActionResult> DeleteDonationRecord(int id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var donationRecordDB = await _db.donationRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (donationRecordDB == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }
            if (donationRecordDB.status == "Confirm")
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

        [HttpPost]
        public async Task<IActionResult> ResetDonationRecord(int id)
        {
            //await _db.donationRecords.AddAsync(donationRecord);
            var donationRecordDB = await _db.donationRecords.FirstOrDefaultAsync(u => u.dID == id);
            if (donationRecordDB == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            if (donationRecordDB.status == "Confirm")
            {
                donationRecordDB.status = "Not Confirm";
                _db.donationRecords.Update(donationRecordDB);
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Confirm Successful" });
            }

            else
                return Json(new { success = false, message = "Donation Status is already Not Confirm" });
        }

        // create archive for current donation record
        [HttpPost]
        public async Task<IActionResult> ArchiveDonationRecords()
        {
            List<DonationRecord> donationRecordList = await _db.donationRecords.ToListAsync();
            int count = 0;
            for (int i = 0; i < donationRecordList.Count(); i++)
            {
                var check = await _db.donationRecordArchives.FirstOrDefaultAsync(u => u.dID == donationRecordList[i].dID);
                if (check == null)
                {
                    donationRecordArchive = new DonationRecordArchive();
                    donationRecordArchive.dateOfArchive = DateTime.Now;
                    donationRecordArchive.dID = donationRecordList[i].dID;
                    donationRecordArchive.name = donationRecordList[i].name;
                    donationRecordArchive.nric = donationRecordList[i].nric;
                    donationRecordArchive.type = donationRecordList[i].type;
                    donationRecordArchive.status = donationRecordList[i].status;
                    donationRecordArchive.doneBy = donationRecordList[i].doneBy;
                    donationRecordArchive.address = donationRecordList[i].address;
                    donationRecordArchive.contactNo = donationRecordList[i].contactNo;
                    donationRecordArchive.taxExemption = donationRecordList[i].taxExemption;
                    donationRecordArchive.dateOfDonation = donationRecordList[i].dateOfDonation;
                    donationRecordArchive.amountOfDonation = donationRecordList[i].amountOfDonation;
                    donationRecordArchive.receiptNo = donationRecordList[i].receiptNo;
                    donationRecordArchive.dateOfReceipt = donationRecordList[i].dateOfReceipt;
                    donationRecordArchive.modeOfDonation = donationRecordList[i].modeOfDonation;
                    donationRecordArchive.purposeOfDonation = donationRecordList[i].purposeOfDonation;
                    donationRecordArchive.remarks = donationRecordList[i].remarks;
                    _db.donationRecordArchives.Add(donationRecordArchive);
                    count++;
                    await _db.SaveChangesAsync();
                }
            }
            if (count > 0)
                return Json(new { success = true, message = "Archive Successful" });
            else
                return Json(new { success = false, message = "Archive is up to date" });
        }

        // create archive for current donor record

        public async Task<IActionResult> ArchiveDonorRecords()
        {
            List<DonorRecord> donorRecordList = await _db.donorRecords.ToListAsync();
            int count = 0;

            for (int i = 0; i < donorRecordList.Count(); i++)
            {
                if (_db.donorRecordArchives.Find(donorRecordList[i].dID) == null)
                {
                    donorRecordArchive = new DonorRecordArchive();
                    donorRecordArchive.archiveID = donorRecordList[i].dID;
                    donorRecordArchive.dateOfArchive = DateTime.Now;
                    donorRecordArchive.dID = donorRecordList[i].dID;
                    donorRecordArchive.name = donorRecordList[i].name;
                    donorRecordArchive.nric = donorRecordList[i].nric;
                    donorRecordArchive.address = donorRecordList[i].address;
                    donorRecordArchive.contactno = donorRecordList[i].contactno;
                    donorRecordArchive.username = donorRecordList[i].username;
                    donorRecordArchive.password = donorRecordList[i].password;
                    _db.donorRecordArchives.Add(donorRecordArchive);
                    count++;
                }

            }
            await _db.SaveChangesAsync();
            if (count > 0)
                return Json(new { success = true, message = "Archive Successful" });
            else
                return Json(new { success = false, message = "Archive is up to date" });
        }

        #endregion
    }
}
