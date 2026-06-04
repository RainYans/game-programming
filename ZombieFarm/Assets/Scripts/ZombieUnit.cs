using System;

public enum HungerState
{
    Full,   // freshly harvested / recently fed — weaker in combat
    Hungry, // left alone long enough — stronger in combat
}

/// One owned zombie. Unlike fungible seeds, each unit is tracked individually so it can carry
/// its own hunger state and (later) be deployed and permanently lost. Hunger is derived from
/// wall-clock time the same way crop growth is, so it stays correct across saves: a unit is
/// Full right after harvest/feeding and drifts to Hungry once the delay elapses.
[Serializable]
public class ZombieUnit
{
    public string uid;
    public string strainId;
    public long becameFullAtUtcMs;

    public ZombieUnit(string strainId, DateTime becameFullUtc, string uid = null)
    {
        this.strainId = strainId;
        this.uid = string.IsNullOrEmpty(uid) ? Guid.NewGuid().ToString("N") : uid;
        becameFullAtUtcMs = new DateTimeOffset(becameFullUtc, TimeSpan.Zero).ToUnixTimeMilliseconds();
    }

    public DateTime BecameFullUtc =>
        DateTimeOffset.FromUnixTimeMilliseconds(becameFullAtUtcMs).UtcDateTime;

    /// Mark the unit Full again (harvest, feeding, or a Hunger-clearing item), restarting the
    /// drift toward Hungry from now.
    public void Feed() =>
        becameFullAtUtcMs = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero).ToUnixTimeMilliseconds();

    public HungerState State(float hungerDelaySeconds)
    {
        double elapsed = (DateTime.UtcNow - BecameFullUtc).TotalSeconds;
        return elapsed >= hungerDelaySeconds ? HungerState.Hungry : HungerState.Full;
    }
}
