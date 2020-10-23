using Doppler.Sap.Factory;
using Doppler.Sap.Models;
using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public class SapService : ISapService
    {
        private readonly ISapTaskFactory _sapTaskFactory;

        public SapService(ISapTaskFactory sapTaskFactory)
        {
            _sapTaskFactory = sapTaskFactory;
        }

        public async Task<SapTaskResult> SendToSap(SapTask dequeuedTask)
        {
            return await _sapTaskFactory.CreateHandler(dequeuedTask);
        }
    }
}
