using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PlatformaDeStiri.Models
{
    public class Category
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<News> News { get; set; }
    }
}