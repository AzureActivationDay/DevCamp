namespace Data_Platform_Demos.DocumentDB
{
  public class Subcategory
  {
    public int SubcategoryId { get; set; }
    public string Name { get; set; }
    public Category Category { get; set; }
  }
}