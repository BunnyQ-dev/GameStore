public class AchievementDto
{
  public int Id { get; set; }
  public string Name { get; set; }
  public string Description { get; set; }
  public string IconUrl { get; set; }
  public int? GameId { get; set; } // Added GameId property
}
