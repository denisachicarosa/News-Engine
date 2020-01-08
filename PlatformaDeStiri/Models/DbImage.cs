using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace PlatformaDeStiri.Models
{
    public class DbImage
    {
        [Key]
        public int ImageID { get; set; }
        
        public string Title { get; set; }

        [DataType(DataType.Upload)]
        [Display(Name = "Upload File")]
        [Required(ErrorMessage = "Please choose file to upload.")]
        public string ImagePath { get; set; }


        [NotMapped]
        public HttpPostedFileBase ImageFile { get; set; }
    }
}