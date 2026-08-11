using System.Data;

namespace Labor.DataAccess.Context
{
    public interface IDbContext
    {
        IDbConnection CreateConnection();
    }
} 