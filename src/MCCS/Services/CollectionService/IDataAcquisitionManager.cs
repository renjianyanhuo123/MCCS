using MCCS.Core.Devices;
using MCCS.Core.Devices.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCCS.Services.CollectionService
{
    public interface IDataAcquisitionManager : IDisposable
    {
        public IObservable<DataCollectionError> ErrorStream { get; }

        public void StartCollection(string deviceId, TimeSpan? interval = null);

        public void StopCollection(string deviceId);

        public void StartAllCollection(TimeSpan? interval = null);

        public void StopAllCollection();
    }
}
