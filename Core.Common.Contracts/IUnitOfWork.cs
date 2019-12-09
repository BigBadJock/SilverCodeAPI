namespace Core.Common.Contracts
{
    public interface IUnitOfWork
    {
        void Commit();
    }
}
