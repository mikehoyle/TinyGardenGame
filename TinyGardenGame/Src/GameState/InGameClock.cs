using System;
using TinyGardenGame.Config;

namespace TinyGardenGame.GameState {
  public class InGameClock {
    private readonly int _hoursOfDaylight;
    public int Day { get; private set; }
    public double TimeOfDay { get; private set; }

    public bool IsNight { get; private set; }

    /** All events send day number */
    public event EventHandler<int> OnNewDay;

    public event EventHandler<int> OnNightBegin;

    public InGameClock() {
      Day = 1;
      TimeOfDay = 0;
      IsNight = false;
      _hoursOfDaylight = GameConfig.Config.TotalHoursInADay - GameConfig.Config.HoursOfNight;
    }

    public void Update(GameTime gameTime) {
      TimeOfDay += gameTime.ElapsedGameTime.TotalSeconds / GameConfig.Config.HourLengthInSeconds;
      if (TimeOfDay >= GameConfig.Config.TotalHoursInADay) {
        Day += 1;
        // Allow for overflow into the next day
        TimeOfDay -= GameConfig.Config.TotalHoursInADay;
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