using System;

namespace TinyGardenGame.Plants.Components {
  public class GrowthComponent {
    // Total time to grow, in seconds
    public TimeSpan GrowthTime { get; set; }
    public double CurrentGrowthPercentage { get; set; }
    public bool IsFullyGrown { get; set; } = false;

    public GrowthComponent(TimeSpan growthTime) {
      GrowthTime = growthTime;
    }

    /**
     * Increments growth by the given timespan.
     * <returns>Whether growth has fully completed as a result of this call</returns>
     */
    public bool IncrementGrowth(TimeSpan timePassed) {
      CurrentGrowthPercentage += (timePassed / GrowthTime);
      if (CurrentGrowthPercentage > 1.0) {
        CurrentGrowthPercentage = 1.0;
        IsFullyGrown = true;
      }
      
      return IsFullyGrown;
    }
  }
}