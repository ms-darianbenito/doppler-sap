using System.Threading.Tasks;

namespace Doppler.Sap.Services
{
    public interface ISlackService
    {
        public Task SendNotification(string message = null);
    }
}
