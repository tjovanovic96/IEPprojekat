namespace IEPprojekat.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Order")]
    public partial class Order
    {
        [Key]
        public Guid IdOrder { get; set; }

        [Display(Name = "Number of tokens")]
        public int? NumberOfTokens { get; set; }

        [Display(Name = "Real price")]
        public decimal? RealPrice { get; set; }

        [StringLength(255)]
        [Display(Name = "Current state")]
        public string CurrentState { get; set; }

        [Required]
        [StringLength(128)]
        public string IdUser { get; set; }

        public virtual User User { get; set; }
    }
}
