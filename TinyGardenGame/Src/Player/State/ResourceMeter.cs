using System;

namespace TinyGardenGame.Player.State {
  public class ResourceMeter {
    private double _currentValue;
    public double MaxValue { get; set; }
    public double MinValue { get; }


    public double CurrentValue {
      get => _currentValue;
      set => _currentValue = Math.Clamp(value, MinValue, MaxValue);
    }

    public double PercentFull => Math.Clamp(CurrentValue / MaxValue, 0, 1);

    public event EventHandler ResourceDepleted;

    public ResourceMeter(int maxValue, int minValue = 0) {
      MaxValue = maxValue;
      MinValue = minValue;
      CurrentValue = maxValue;
    }

    /**
     * <returns>Whether the resource was depleted as a result of this call</returns>
     */
    public bool DecreaseBy(double amount) {
      return IncreaseBy(-amount);
    }

    /**
     * <returns>Whether the resource was depleted as a result of this call</returns>
     */
    public bool IncreaseBy(double amount) {
      if (CurrentValue > MinValue && (CurrentValue + amount) <= MinValue) {
        CurrentValue = MinValue;
        ResourceDepleted?.Invoke(this, EventArgs.Empty);
        return true;
      }

      CurrentValue += amount;
      return false;
    }
  }
}