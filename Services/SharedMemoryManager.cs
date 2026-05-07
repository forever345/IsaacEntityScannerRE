using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace IsaacEntityScannerRE.Services;

[StructLayout(LayoutKind.Sequential)]
public struct EntityState
{
    public int ptr;
    public int type;
    public int variant;
    public int id;
}

public class SharedMemoryManager
{
    private MemoryMappedFile _mmf;
    private MemoryMappedViewAccessor _accessor;

    private const string MapName = "Local\\MyEntitySharedMemory";

    // layout MUST match C++
    private const int WriteIndexOffset = 0;
    private const int PublishIndexOffset = 4;

    private const int EntitiesOffset = 8;

    private const int EntitySize = 16; // 4 + 4 + 4 + 4
    private const int MaxEntities = 1024;

    private int _lastPublish;
    private int _lastReadIndex = -1;


    public void Init()
    {
        _mmf = MemoryMappedFile.OpenExisting(MapName);
        _accessor = _mmf.CreateViewAccessor();
    }

    /* Old method for read all snapshot structure
    public bool HasNewData()
    {
        int pub = _accessor.ReadInt32(PublishIndexOffset);

        if (pub == _lastPublish)
            return false;

        _lastPublish = pub;
        return true;
    }*/

    public bool HasNewData()
    {
        int pub = _accessor.ReadInt32(PublishIndexOffset);

        if (pub < 0)
            return false;

        return pub != _lastReadIndex;
    }

    public List<EntityState> ReadNew()
    {
        var result = new List<EntityState>();

        int publishIndex = _accessor.ReadInt32(PublishIndexOffset);

        if (publishIndex < 0)
            return result;

        if (_lastReadIndex == publishIndex)
            return result;

        while (true)
        {
            _lastReadIndex++;
            _lastReadIndex %= MaxEntities;

            var e = ReadEntity(_lastReadIndex);

            if (e.ptr != 0 && e.id > 0)
            {
                result.Add(e);
            }

            if (_lastReadIndex == publishIndex)
                break;
        }

        return result;
    }

    public EntityState ReadEntity(int index)
    {
        long offset = EntitiesOffset + (index * EntitySize);

        return new EntityState
        {
            ptr = _accessor.ReadInt32(offset + 0),
            type = _accessor.ReadInt32(offset + 4),
            variant = _accessor.ReadInt32(offset + 8),
            id = _accessor.ReadInt32(offset + 12)
        };
    }


    public List<EntityState> ReadAll()
    {
        var result = new List<EntityState>();

        for (int i = 0; i < MaxEntities; i++)
        {
            var e = ReadEntity(i);

            if (e.ptr == 0 || e.id <= 0)
                continue;

            result.Add(e);
        }

        return result;
    }

}
