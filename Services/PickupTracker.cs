using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IsaacEntityScannerRE.Services;

public class PickupTracker
{
    // ===== STATE =====
    private readonly HashSet<PickupKey> _current = new();
    private readonly HashSet<PickupKey> _all = new();

    // ===== EVENT =====
    public event Action<PickupUpdate>? OnUpdated;

    // ===== PUBLIC API =====
    public void Update(IEnumerable<EntityState> entities)
    {
        var newSet = new HashSet<PickupKey>();

        foreach (var e in entities)
        {
            // minimalna walidacja (śmieci z SHM)
            if (e.ptr == 0)
                continue;

            if (e.id <= 0)
                continue;

            newSet.Add(new PickupKey(e.variant, e.id));
        }

        // brak zmian → nic nie rób
        if (newSet.SetEquals(_current))
            return;

        // diff
        var added = newSet.Except(_current).ToList();
        var removed = _current.Except(newSet).ToList();

        // update state
        _current.Clear();
        foreach (var item in newSet)
            _current.Add(item);

        foreach (var item in newSet)
            _all.Add(item);

        // emit event
        OnUpdated?.Invoke(new PickupUpdate
        {
            Current = _current.ToList(),
            All = _all.ToList(),
            Added = added.ToList(),
            Removed = removed.ToList()
        });
    }

    // ===== READ ONLY ACCESS =====
    public IReadOnlyCollection<PickupKey> Current => _current;
    public IReadOnlyCollection<PickupKey> All => _all;

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
    public List<PickupTracker.PickupKey> Current { get; set; } = new();
    public List<PickupTracker.PickupKey> All { get; set; } = new();

    public List<PickupTracker.PickupKey> Added { get; set; } = new();
    public List<PickupTracker.PickupKey> Removed { get; set; } = new();
}
