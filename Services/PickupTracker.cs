using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static IsaacEntityScannerRE.Services.PickupTracker;

namespace IsaacEntityScannerRE.Services;

public class PickupTracker
{
    // ===== STATE =====
    private readonly Dictionary<PickupKey, DateTime> _recent = new();
    private readonly HashSet<PickupKey> _seen = new();

    // ===== EVENT =====
    public event Action<PickupUpdate>? OnUpdated;

    // ===== PUBLIC API =====
    public void Update(IEnumerable<EntityState> entities)
    {
        var now = DateTime.UtcNow;

        var recentAdded = new List<PickupKey>();
        var dismissed = new List<PickupKey>();

        foreach (var e in entities)
        {
            if (e.ptr == 0)
                continue;

            if (e.id <= 0)
                continue;

            var key = new PickupKey(e.variant, e.id);

            // seen
            _seen.Add(key);

            // recent refresh/add
            if (!_recent.ContainsKey(key))
            {
                recentAdded.Add(key);
            }

            _recent[key] = now;
        }

        var expired = _recent
            .Where(x => now - x.Value > TimeSpan.FromSeconds(10))
            .Select(x => x.Key)
            .ToList();

        foreach (var key in expired)
        {
            _recent.Remove(key);
            dismissed.Add(key);
        }

        // emit event
        OnUpdated?.Invoke(new PickupUpdate
        {
            Recent = _recent
                .OrderBy(x => x.Value)
                .Select(x => x.Key)
                .ToList(),
            Seen = _seen.ToList(),

            RecentAdded = recentAdded,
            Dismissed = dismissed
        });
    }

    // ===== READ ONLY ACCESS =====
    public IReadOnlyCollection<PickupKey> Recent => _recent.OrderBy(x => x.Value).Select(x => x.Key).ToList();
    public IReadOnlyCollection<PickupKey> Seen => _seen;

    // ===== INTERNAL KEY =====
    public readonly struct PickupKey : IEquatable<PickupKey>
    {
        public int Variant { get; }
        public int Id { get; }

        public PickupKey(int variant, int id)
        {
            Variant = variant;
            Id = id;
        }

        public bool Equals(PickupKey other)
            => Variant == other.Variant && Id == other.Id;

        public override bool Equals(object? obj)
            => obj is PickupKey other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(Variant, Id);

        public override string ToString()
            => $"{Variant}:{Id}";
    }
}

// ===== UPDATE PAYLOAD =====
public class PickupUpdate
{
    public List<PickupTracker.PickupKey> Recent { get; set; } = new();
    public List<PickupTracker.PickupKey> Seen { get; set; } = new();

    public List<PickupTracker.PickupKey> RecentAdded { get; set; } = new();
    public List<PickupTracker.PickupKey> Dismissed { get; set; } = new();
}
