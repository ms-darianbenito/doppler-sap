using System.Threading.Tasks;
using Doppler.Sap.Models;

namespace Doppler.Sap.Factory
{
    public interface ISapTaskFactory
    {
        Task<SapTaskResult> CreateHandler(SapTask sapTaskType);
    }
}
