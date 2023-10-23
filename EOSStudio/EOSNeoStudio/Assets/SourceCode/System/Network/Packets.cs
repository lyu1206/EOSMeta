using System;
using System.CodeDom;
using System.Collections.Generic;
using Battlehub.RTCommon;
using Eos.Objects.Description;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePack.Unity;
using UnityEngine;

[MessagePackObject]
public class CreateObjectPacket
{
    [Key(1)]
    public List<ObjectDescription> description;
}
[MessagePackObject]
public class RequestPlayertPacket
{
    [Key(1)]
    public int playerID;
}

public class NotifyPropertyValueFormmater : MessagePack.Formatters.IMessagePackFormatter<object>
{
    public void Serialize(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options)
    {
        // var formmater = options.Resolver.GetFormatter<Vector3Formatter>();
        // formmater.Serialize(ref writer,(UnityEngine.Vector3)value.value,options);

        var type = value.GetType();
        if (type == typeof(Vector3))
        {
            writer.WriteInt8(1);
            UnityResolver.Instance.GetFormatter<Vector3>()?.Serialize(ref writer,(Vector3)value,options);
        }
        else if (type == typeof(float))
        {
            writer.WriteInt8(2);
            StandardResolver.Instance.GetFormatter<float>()?.Serialize(ref writer,(float)value,options);
        }
        else if (type == typeof(int))
        {
            writer.WriteInt8(3);
            StandardResolver.Instance.GetFormatter<int>()?.Serialize(ref writer,(int)value,options);
        }
        else if (type == typeof(string))
        {
            writer.WriteInt8(4);
            StandardResolver.Instance.GetFormatter<string>()?.Serialize(ref writer,(string)value,options);
        }
        else if (type == typeof(bool))
        {
            writer.WriteInt8(5);
            StandardResolver.Instance.GetFormatter<bool>()?.Serialize(ref writer,(bool)value,options);
        }
        else
            throw new Exception($"not permitted notify property type.Type:{type}");
    }

    public object Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var type = reader.ReadByte();
        if (type == 1)
        {
            return UnityResolver.Instance.GetFormatter<Vector3>().Deserialize(ref reader,options);
        }
        else if (type == 2)
        {
            return StandardResolver.Instance.GetFormatter<float>().Deserialize(ref reader,options);
        }
        else if (type == 3)
        {
            return StandardResolver.Instance.GetFormatter<int>().Deserialize(ref reader,options);
        }
        else if (type == 4)
        {
            return StandardResolver.Instance.GetFormatter<string>().Deserialize(ref reader,options);
        }
        else if (type == 5)
        {
            return StandardResolver.Instance.GetFormatter<bool>().Deserialize(ref reader,options);
        }
        throw new Exception($"not permitted notify property type.");
    }
}

[MessagePackObject]
public class NotifyPropertyPacket
{
    [MessagePackObject]
    public class NotifyPropertyItem
    {
        [Key(21)] 
        public short notify;
        [Key(22)]
        [MessagePackFormatter(typeof(NotifyPropertyValueFormmater))]
        public object value;
    }

    [Key(1)] public uint objectid;

    [Key(2)] public List<NotifyPropertyItem> properties;
}

[MessagePackObject]
public class NotifyObjectPacket
{
    [Key(1)]
    public List<NotifyPropertyPacket> notifies = new List<NotifyPropertyPacket>();
}

[MessagePackObject]
public class NotifyObjectActionPacket
{
    [Key(1)] 
    public uint objectid;
    [Key(2)] 
    public short notifyaction;
    [Key(3)]
    public object[] parameters;
}
