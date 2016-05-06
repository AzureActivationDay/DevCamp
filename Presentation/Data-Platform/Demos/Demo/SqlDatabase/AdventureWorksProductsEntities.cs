using System.Data.Common;
using System.Data.Entity.Core.EntityClient;

namespace Data_Platform_Demos.SqlDatabase
{
  public partial class AdventureWorksProductsEntities
  {
    public AdventureWorksProductsEntities(EntityConnection entityConnection) : base(entityConnection, true) { }
  }
}