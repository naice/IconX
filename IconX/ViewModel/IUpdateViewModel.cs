using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IconX.ViewModel
{
    interface IUpdateViewModel
    {
        TimeSpan UpdateTimeout { get; set; }
        DateTime LastUpdate { get; set; }
        SemaphoreSlim UILock { get; set; }

        Task<object> ProcessData();

        void UpdateUI(object data);
    }
}
