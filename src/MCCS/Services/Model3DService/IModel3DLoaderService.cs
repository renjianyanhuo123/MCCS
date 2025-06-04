namespace MCCS.Services.Model3DService
{
    public interface IModel3DLoaderService
    {
        public void CancelImport();

        bool IsImporting { get; }
    }
}
