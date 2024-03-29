﻿using DbManager.Data.Relations;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data.DTOs
{
    public class OrderOutDTO
    {
        public Guid Id { get; set; }
        public double Price { get; set; }
        public int SumWeight { get; set; }
        public string DeliveryAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string? Review { get; set; }
        public int? ClientRating { get; set; }

        public List<OrderStateItemOutDTO> Story { get; set; } = new List<OrderStateItemOutDTO>();
    }
}
