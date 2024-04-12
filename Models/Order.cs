using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static Intex.Models.Cart;

namespace Intex.Models
{
    public partial class Order
    {
        [Key]
        public int TransactionId { get; set; }

        public int? CustomerId { get; set; }

        public string? Date { get; set; }

        public string? DayOfWeek { get; set; }

        public int? Time { get; set; }

        public string? EntryMode { get; set; }

        public int? Amount { get; set; }

        public string? TypeOfTransaction { get; set; }

        public string? CountryOfTransaction { get; set; }

        public string? ShippingAddress { get; set; }

        public string? Bank { get; set; }

        public string? TypeOfCard { get; set; }

        public int? Fraud { get; set; }

        // Define CartLines as a collection of CartLine entities
        public ICollection<Cartline> CartLines { get; set; }
    }
}

