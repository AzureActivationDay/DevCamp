using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Data_Platform_Demos.DocumentDB
{
  public class Model
  {
    public string CatalogDescription { get; set; }
    public string Instructions { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Name { get; set; }
    public Description[] Description { get; set; }
  }
}