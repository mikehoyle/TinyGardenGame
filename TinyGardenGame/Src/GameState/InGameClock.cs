using System;

namespace TinyGardenGame.GameState {
  public class InGameClock {
    private readonly Config.Config _config;
    private readonly int _hoursOfDaylight;
    public int Day { get; private set; }
    public double TimeOfDay { get; private set; }

    public bool IsNight { get; private set; }

    /** All events send day number */
    public event EventHandler<int> OnNewDay;

    public event EventHandler<int> OnNightBegin;

    public InGameClock(Config.Config config) {
      Day = 1;
      TimeOfDay = 0;
      IsNight = false;
      _config = config;
      _hoursOfDaylight = config.TotalHoursInADay - config.HoursOfNight;
    }

    public void Update(GameTime gameTime) {
      TimeOfDay += gameTime.ElapsedGameTime.TotalSeconds / _config.HourLengthInSeconds;
      if (TimeOfDay >= _config.TotalHoursInADay) {
        Day += 1;
        // Allow for overflow into the next day
        TimeOfDay -= _config.TotalHoursInADay;
        IsNight = false;
        OnNewDay?.Invoke(this, Day);
      }
      else if (!IsNight && TimeOfDay >= _hoursOfDaylight) {
        IsNight = true;
        OnNightBegin?.Invoke(this, Day);
      }
    }
  }
}