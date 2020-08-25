using UnityEngine;
using Normal.Realtime.Serialization;

[RealtimeModel]
public partial class RibbonPointModel
{
    [RealtimeProperty(1,true)]
    private Vector3 _position;
    [RealtimeProperty(2,true)]
    private Quaternion _rotation = Quaternion.identity;
}

/* ----- Begin Normal Autogenerated Code ----- */
public partial class RibbonPointModel : IModel {
    // Properties
    public UnityEngine.Vector3 position {
        get { return _cache.LookForValueInCache(_position, entry => entry.positionSet, entry => entry.position); }
        set { if (value == position) return; _cache.UpdateLocalCache(entry => { entry.positionSet = true; entry.position = value; return entry; }); }
    }
    public UnityEngine.Quaternion rotation {
        get { return _cache.LookForValueInCache(_rotation, entry => entry.rotationSet, entry => entry.rotation); }
        set { if (value == rotation) return; _cache.UpdateLocalCache(entry => { entry.rotationSet = true; entry.rotation = value; return entry; }); }
    }
    
    // Delta updates
    private struct LocalCacheEntry {
        public bool                   positionSet;
        public UnityEngine.Vector3    position;
        public bool                   rotationSet;
        public UnityEngine.Quaternion rotation;
    }
    
    private LocalChangeCache<LocalCacheEntry> _cache;
    
    public RibbonPointModel() {
        _cache = new LocalChangeCache<LocalCacheEntry>();
    }
    
    // Serialization
    enum PropertyID {
        Position = 1,
        Rotation = 2,
    }
    
    public int WriteLength(StreamContext context) {
        int length = 0;
        
        if (context.fullModel) {
            // Mark unreliable properties as clean and flatten the in-flight cache.
            // TODO: Move this out of WriteLength() once we have a prepareToWrite method.
            _position = position;
            _rotation = rotation;
            _cache.Clear();
            
            // Write all properties
            length += WriteStream.WriteBytesLength((uint)PropertyID.Position, WriteStream.Vector3ToBytesLength());
            length += WriteStream.WriteBytesLength((uint)PropertyID.Rotation, WriteStream.QuaternionToBytesLength());
        } else {
            // Reliable properties
            if (context.reliableChannel) {
                LocalCacheEntry entry = _cache.localCache;
                if (entry.positionSet)
                    length += WriteStream.WriteBytesLength((uint)PropertyID.Position, WriteStream.Vector3ToBytesLength());
                if (entry.rotationSet)
                    length += WriteStream.WriteBytesLength((uint)PropertyID.Rotation, WriteStream.QuaternionToBytesLength());
            }
        }
        
        return length;
    }
    
    public void Write(WriteStream stream, StreamContext context) {
        if (context.fullModel) {
            // Write all properties
            stream.WriteBytes((uint)PropertyID.Position, WriteStream.Vector3ToBytes(_position));
            stream.WriteBytes((uint)PropertyID.Rotation, WriteStream.QuaternionToBytes(_rotation));
        } else {
            // Reliable properties
            if (context.reliableChannel) {
                LocalCacheEntry entry = _cache.localCache;
                if (entry.positionSet || entry.rotationSet)
                    _cache.PushLocalCacheToInflight(context.updateID);
                
                if (entry.positionSet)
                    stream.WriteBytes((uint)PropertyID.Position, WriteStream.Vector3ToBytes(entry.position));
                if (entry.rotationSet)
                    stream.WriteBytes((uint)PropertyID.Rotation, WriteStream.QuaternionToBytes(entry.rotation));
            }
        }
    }
    
    public void Read(ReadStream stream, StreamContext context) {
        // Remove from in-flight
        if (context.deltaUpdatesOnly && context.reliableChannel)
            _cache.RemoveUpdateFromInflight(context.updateID);
        
        // Loop through each property and deserialize
        uint propertyID;
        while (stream.ReadNextPropertyID(out propertyID)) {
            switch (propertyID) {
                case (uint)PropertyID.Position: {
                    _position = ReadStream.Vector3FromBytes(stream.ReadBytes());
                    break;
                }
                case (uint)PropertyID.Rotation: {
                    _rotation = ReadStream.QuaternionFromBytes(stream.ReadBytes());
                    break;
                }
                default:
                    stream.SkipProperty();
                    break;
            }
        }
    }
}
/* ----- End Normal Autogenerated Code ----- */
