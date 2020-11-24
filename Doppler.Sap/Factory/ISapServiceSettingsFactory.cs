namespace Doppler.Sap.Factory
{
    public interface ISapServiceSettingsFactory
    {
        ISapTaskHandler CreateHandler(string sapSystem);
    }
}
