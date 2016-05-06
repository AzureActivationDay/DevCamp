// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable InconsistentNaming
namespace Data_Platform_Demos.DocumentDB
{
	public class Product
	{
		public string id { get; set; }
		public string Name { get; set; }
		public string Line { get; set; }
		public Model Model { get; set; }
		public string Number { get; set; }
		public Review[] Reviews { get; set; }
		public Subcategory Subcategory { get; set; }
	}
}