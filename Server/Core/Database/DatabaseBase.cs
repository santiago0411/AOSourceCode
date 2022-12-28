using System.Data.Common;
using SqlKata.Compilers;

namespace AO.Core.Database
{
    public abstract class DatabaseBase
    {
        public abstract DbConnection GetConnection();
        public abstract Compiler GetCompiler();
    }
}
