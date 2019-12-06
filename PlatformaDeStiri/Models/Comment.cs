using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformaDeStiri.Models
{
    public class Comment
    {
        [Key]
        public int commID { get; set; }
        public string commContent { get; set; }
        public DateTime commDate { get; set; }
        public string commUserID { get; set; }
        public int commNewsID { get; set; }


        public virtual News news { get; set; }
        public virtual ApplicationUser user { get; set; }
    }
}