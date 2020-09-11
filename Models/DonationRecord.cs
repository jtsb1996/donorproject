using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DonorProject.Models
{
    public class DonationRecord
    {
        // if you want to do any editting to the database table you would need to re run the sql script again
        // via nuget manager console
        // type add-migration 
        // type update database
        [Key]
        public int dID { get; set; }
        public string doneBy { get; set; }
        [Required]
        public string name { get; set; }
        [Required]
        public string nric { get; set; }
        [Required]
        public string type { get; set; }
        [Required]
        public string status { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        public string contactNo { get; set; }
        [Required]
        public string taxExemption { get; set; }
        [Required]
        public DateTime dateOfDonation { get; set; }
        [Required]
        public int amountOfDonation { get; set; }
        [Required]
        public string receiptNo { get; set; }
        [Required]
        public DateTime dateOfReceipt { get; set; }
        [Required]
        public string modeOfDonation { get; set; }
        [Required]
        public string purposeOfDonation { get; set; }
        public string remarks { get; set; }
    }
}
