using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbManager.Data
{
    [Flags]
    public enum OrderStateEnum
    {
        InQueue = 1,
        Cooking = 2,
        WaitDeliveryMan = 4,
        Delivering = 8,
        Finished = 16,
        Cancelled = 32,
    }
}
