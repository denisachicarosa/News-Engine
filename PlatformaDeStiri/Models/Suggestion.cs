using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformaDeStiri.Models
{
    public class Suggestion
    {
        [Key]
        public int suggID { get; set; }
        public string suggContent { get; set; }
        public DateTime suggDate { get; set; }
        public string suggserID { get; set; }
        public int suggNewsID { get; set; }
       
        public int suggState { get; set; }
        public enum states {Pending, Accepted, Rejected };
    }
}