using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UIExample.ViewModels
{
    public class CartViewModel
    {
        [Required]
        public string User { get; set; }

        public string Product { get; set; }
                
    }
}
