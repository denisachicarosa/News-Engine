using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PlatformaDeStiri.Models
{
    public class News
    {
        [Key]
        public int ID { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public string Content { get; set; }
        public string UserID { get; set; }
        public int CategoryID { get; set; }
        public string suggestedUser { get; set; }
        public int ImageID { get; set; }

        public virtual DbImage Image { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Category Category { get; set; }
        public virtual IEnumerable<SelectListItem> Categories { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Suggestion> Suggestions { get; set; }
    }

}