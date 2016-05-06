using System;

namespace Data_Platform_Demos.DocumentDB
{
  public class Review
  {
    public string Comments { get; set; }
    public string EmailAddress { get; set; }
    public DateTime ModifiedDate { get; set; }
    public int Rating { get; set; }
    public DateTime ReviewDate { get; set; }
    public string ReviewerName { get; set; }
  }
}