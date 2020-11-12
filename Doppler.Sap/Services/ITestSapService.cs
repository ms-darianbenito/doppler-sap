using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public interface ITestSapService
    {
        Task<string> TestSapConnection();

        Task<string> TestSapUsConnection();
    }
}
