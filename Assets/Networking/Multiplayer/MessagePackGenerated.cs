// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    using System;

    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<Type, int>(15)
            {
                { typeof(global::System.Collections.Generic.List<global::SerializableTransform>), 0 },
                { typeof(global::BaseNonVRPlayer.PlayerInfo), 1 },
                { typeof(global::GameSystemData), 2 },
                { typeof(global::MultiBaseRequest), 3 },
                { typeof(global::MultiChangeSceneRequest), 4 },
                { typeof(global::MultiDespawnObject), 5 },
                { typeof(global::MultiInitialData), 6 },
                { typeof(global::MultiNewConnection), 7 },
                { typeof(global::MultiSpawnPlayer), 8 },
                { typeof(global::MultiSpawnRequest), 9 },
                { typeof(global::MultiSyncPlayer), 10 },
                { typeof(global::MultiSyncRequest), 11 },
                { typeof(global::SerializableQuaternion), 12 },
                { typeof(global::SerializableTransform), 13 },
                { typeof(global::SerializableVector3), 14 },
            };
        }

        internal static object GetFormatter(Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new global::MessagePack.Formatters.ListFormatter<global::SerializableTransform>();
                case 1: return new MessagePack.Formatters.BaseNonVRPlayer_PlayerInfoFormatter();
                case 2: return new MessagePack.Formatters.GameSystemDataFormatter();
                case 3: return new MessagePack.Formatters.MultiBaseRequestFormatter();
                case 4: return new MessagePack.Formatters.MultiChangeSceneRequestFormatter();
                case 5: return new MessagePack.Formatters.MultiDespawnObjectFormatter();
                case 6: return new MessagePack.Formatters.MultiInitialDataFormatter();
                case 7: return new MessagePack.Formatters.MultiNewConnectionFormatter();
                case 8: return new MessagePack.Formatters.MultiSpawnPlayerFormatter();
                case 9: return new MessagePack.Formatters.MultiSpawnRequestFormatter();
                case 10: return new MessagePack.Formatters.MultiSyncPlayerFormatter();
                case 11: return new MessagePack.Formatters.MultiSyncRequestFormatter();
                case 12: return new MessagePack.Formatters.SerializableQuaternionFormatter();
                case 13: return new MessagePack.Formatters.SerializableTransformFormatter();
                case 14: return new MessagePack.Formatters.SerializableVector3Formatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1649 // File name should match first type name




// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1200 // Using directives should be placed correctly
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters
{
    using global::System.Buffers;
    using global::MessagePack;

    public sealed class BaseNonVRPlayer_PlayerInfoFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::BaseNonVRPlayer.PlayerInfo>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::BaseNonVRPlayer.PlayerInfo value, global::MessagePack.MessagePackSerializerOptions options)
        {
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(3);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.position, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Serialize(ref writer, value.rotation, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Serialize(ref writer, value.lookDir, options);
        }

        public global::BaseNonVRPlayer.PlayerInfo Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var __position__ = default(global::UnityEngine.Vector3);
            var __rotation__ = default(global::UnityEngine.Quaternion);
            var __lookDir__ = default(global::UnityEngine.Quaternion);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __position__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        __rotation__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        __lookDir__ = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::BaseNonVRPlayer.PlayerInfo(__position__, __rotation__, __lookDir__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class GameSystemDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::GameSystemData>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::GameSystemData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.S);
            writer.Write(value.D);
        }

        public global::GameSystemData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __S__ = default(int);
            var __D__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __S__ = reader.ReadInt32();
                        break;
                    case 1:
                        __D__ = reader.ReadBytes()?.ToArray();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::GameSystemData(__S__, __D__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiBaseRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiBaseRequest>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiBaseRequest value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.RT);
            writer.Write(value.R);
        }

        public global::MultiBaseRequest Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var ____result = new global::MultiBaseRequest();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.RT = reader.ReadInt32();
                        break;
                    case 1:
                        ____result.R = reader.ReadBytes()?.ToArray();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiChangeSceneRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiChangeSceneRequest>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiChangeSceneRequest value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(1);
            writer.Write(value.N);
        }

        public global::MultiChangeSceneRequest Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __N__ = default(int);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __N__ = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiChangeSceneRequest(__N__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiDespawnObjectFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiDespawnObject>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiDespawnObject value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(1);
            writer.Write(value.ID);
        }

        public global::MultiDespawnObject Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __ID__ = default(int);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __ID__ = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiDespawnObject(__ID__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiInitialDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiInitialData>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiInitialData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(8);
            writer.Write(value.T);
            formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.sOI, options);
            formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.sOK, options);
            formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.gOC, options);
            formatterResolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.gOT, options);
            writer.Write(value.sI);
            writer.Write(value.sOT);
            writer.Write(value.tN);
        }

        public global::MultiInitialData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::MultiInitialData();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.T = reader.ReadInt32();
                        break;
                    case 1:
                        ____result.sOI = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
                        break;
                    case 2:
                        ____result.sOK = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
                        break;
                    case 3:
                        ____result.gOC = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
                        break;
                    case 4:
                        ____result.gOT = formatterResolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
                        break;
                    case 5:
                        ____result.sI = reader.ReadInt32();
                        break;
                    case 6:
                        ____result.sOT = reader.ReadInt32();
                        break;
                    case 7:
                        ____result.tN = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiNewConnectionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiNewConnection>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiNewConnection value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(1);
            writer.Write(value.tN);
        }

        public global::MultiNewConnection Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __tN__ = default(int);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __tN__ = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiNewConnection(__tN__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiSpawnPlayerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiSpawnPlayer>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiSpawnPlayer value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.T);
            writer.Write(value.C);
        }

        public global::MultiSpawnPlayer Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __T__ = default(int);
            var __C__ = default(int);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __T__ = reader.ReadInt32();
                        break;
                    case 1:
                        __C__ = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiSpawnPlayer(__T__, __C__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiSpawnRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiSpawnRequest>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiSpawnRequest value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(1);
            writer.Write(value.I);
        }

        public global::MultiSpawnRequest Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __I__ = default(int);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __I__ = reader.ReadInt32();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiSpawnRequest(__I__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiSyncPlayerFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiSyncPlayer>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiSyncPlayer value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(2);
            writer.Write(value.C);
            writer.Write(value.S);
        }

        public global::MultiSyncPlayer Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __C__ = default(int);
            var __S__ = default(byte[]);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __C__ = reader.ReadInt32();
                        break;
                    case 1:
                        __S__ = reader.ReadBytes()?.ToArray();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::MultiSyncPlayer(__C__, __S__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class MultiSyncRequestFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::MultiSyncRequest>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::MultiSyncRequest value, global::MessagePack.MessagePackSerializerOptions options)
        {
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(2);
            writer.Write(value.ID);
            formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::SerializableTransform>>().Serialize(ref writer, value.tfs, options);
        }

        public global::MultiSyncRequest Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::MultiSyncRequest();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.ID = reader.ReadInt32();
                        break;
                    case 1:
                        ____result.tfs = formatterResolver.GetFormatterWithVerify<global::System.Collections.Generic.List<global::SerializableTransform>>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }

    public sealed class SerializableQuaternionFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SerializableQuaternion>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::SerializableQuaternion value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(4);
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
            writer.Write(value.w);
        }

        public global::SerializableQuaternion Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __x__ = default(float);
            var __y__ = default(float);
            var __z__ = default(float);
            var __w__ = default(float);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __x__ = reader.ReadSingle();
                        break;
                    case 1:
                        __y__ = reader.ReadSingle();
                        break;
                    case 2:
                        __z__ = reader.ReadSingle();
                        break;
                    case 3:
                        __w__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::SerializableQuaternion(__x__, __y__, __z__, __w__);
            reader.Depth--;
            return ____result;
        }
    }

    public sealed class SerializableTransformFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SerializableTransform>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::SerializableTransform value, global::MessagePack.MessagePackSerializerOptions options)
        {
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(2);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Serialize(ref writer, value.p, options);
            formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Serialize(ref writer, value.r, options);
        }

        public global::SerializableTransform Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::SerializableTransform();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.p = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Vector3>().Deserialize(ref reader, options);
                        break;
                    case 1:
                        ____result.r = formatterResolver.GetFormatterWithVerify<global::UnityEngine.Quaternion>().Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }

    public sealed class SerializableVector3Formatter : global::MessagePack.Formatters.IMessagePackFormatter<global::SerializableVector3>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::SerializableVector3 value, global::MessagePack.MessagePackSerializerOptions options)
        {
            writer.WriteArrayHeader(3);
            writer.Write(value.x);
            writer.Write(value.y);
            writer.Write(value.z);
        }

        public global::SerializableVector3 Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                throw new global::System.InvalidOperationException("typecode is null, struct not supported");
            }

            options.Security.DepthStep(ref reader);
            var length = reader.ReadArrayHeader();
            var __x__ = default(float);
            var __y__ = default(float);
            var __z__ = default(float);

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        __x__ = reader.ReadSingle();
                        break;
                    case 1:
                        __y__ = reader.ReadSingle();
                        break;
                    case 2:
                        __z__ = reader.ReadSingle();
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            var ____result = new global::SerializableVector3(__x__, __y__, __z__);
            reader.Depth--;
            return ____result;
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1200 // Using directives should be placed correctly
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name

