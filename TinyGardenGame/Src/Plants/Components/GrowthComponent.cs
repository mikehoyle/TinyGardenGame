using System;

namespace TinyGardenGame.Plants.Components {
  public class GrowthComponent {
    public const string GrowthAnimationPrefix = "growth";
    private const string GrownPostfix = "_full";
    
    // Total time to grow, in seconds
    public TimeSpan GrowthTime { get; set; }
    public int GrowthStages { get; }
    public double CurrentGrowthPercentage { get; set; }
    public bool IsFullyGrown { get; set; } = false;

    public GrowthComponent(TimeSpan growthTime, int growthStages = 1) {
      GrowthTime = growthTime;
      GrowthStages = growthStages;
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

    public string CurrentGrowthAnimationName() {
      if (IsFullyGrown) {
        return $"{GrowthAnimationPrefix}{GrownPostfix}";
      }

      // +1 because sprite animations are 1-indexed
      var currentStage = Convert.ToInt32(
          Math.Floor(CurrentGrowthPercentage * GrowthStages)) + 1;
      return $"{GrowthAnimationPrefix}{currentStage}";
    }
  }
}