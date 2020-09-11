using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DonorProject.Models
{
    public class DonorRecordArchive
    {
        // if you want to do any editting to the database table you would need to re run the sql script again
        // via nuget manager console
        // type add-migration 
        // type update database
        [Key]
        public string archiveID { get; set; }
        public DateTime dateOfArchive { get; set; }
        public string dID { get; set; }
        [Required]
        public string name { get; set; }
        public string nric { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string address { get; set; }
        public string contactno { get; set; }
    }
}
