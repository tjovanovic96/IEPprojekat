namespace IEPprojekat.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("InformationsForAdministrator")]
    public partial class InformationsForAdministrator
    {
        public Guid Id { get; set; }

        public int? ItemsPerPage { get; set; }

        public int? SilverPack { get; set; }

        public int? GoldPack { get; set; }

        public int? PlatinumPack { get; set; }

        public decimal? ValueToken { get; set; }

        [StringLength(50)]
        public string Currency { get; set; }
    }
}
