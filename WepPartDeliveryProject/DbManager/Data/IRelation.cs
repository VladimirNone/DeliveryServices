﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data
{
    public interface IRelation : IModel
    {
        INode NodeFrom { get; set; }
        INode NodeTo { get; set; }

        Guid? NodeFromId { get; set; }
        Guid? NodeToId { get; set; }
    }
}
