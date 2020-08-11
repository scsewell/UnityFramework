using System;

using UnityEngine;

namespace Framework.IO
{
    /// <summary>
    /// Standardizes aspects of binary data serialization.
    /// </summary>
    public abstract class SerializableData
    {
        /// <summary>
        /// The code used to identify this type of serialized data.
        /// </summary>
        protected abstract FourCC SerializerType { get; }

        /// <summary>
        /// The current version number of the serializer.
        /// </summary>
        protected abstract ushort SerializerVersion { get; }

        /// <summary>
        /// Was this instance successfully deserialized.
        /// </summary>
        public bool IsValid { get; private set; } = true;

        /// <summary>
        /// Serializes this data.
        /// </summary>
        /// <param name="writer">The writer to output to.</param>
        /// <returns>True if the serialization was successful.</returns>
        public bool Serialize(DataWriter writer)
        {
            try
            {
                // write the serializer version
                writer.Write(SerializerType);
                writer.Write(SerializerVersion);

                // write the contents
                OnSerialize(writer);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to serialize {GetType().Name}! {e}");
                return false;
            }
        }

        /// <summary>
        /// Serializes this data.
        /// </summary>
        /// <param name="writer">The writer to output to.</param>
        protected abstract void OnSerialize(DataWriter writer);

        /// <summary>
        /// Deserializes data and applies it to this instance.
        /// </summary>
        /// <param name="reader">The reader to get data from.</param>
        /// <returns>True if the deserialization was successful.</returns>
        protected bool Deserialize(DataReader reader)
        {
            try
            {
                // ensure that upcoming bytes are serialized data of this type
                var type = reader.Read<FourCC>();

                if (SerializerType != type)
                {
                    throw new Exception($"Serialized data is {type} but the deserializer is expecting {SerializerType} ({GetType().Name})!");
                }

                // get the serialized format version
                var version = reader.Read<ushort>();

                OnDeserialize(reader, version);
                IsValid = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to deserialize {GetType().Name}! {e}");
                IsValid = false;
            }

            return IsValid;
        }

        /// <summary>
        /// Deserializes data and applies it to this instance.
        /// </summary>
        /// <param name="reader">The reader to get data from.</param>
        /// <param name="version">The serializer version used to write the upcoming data.</param>
        protected abstract void OnDeserialize(DataReader reader, ushort version);
    }
}
