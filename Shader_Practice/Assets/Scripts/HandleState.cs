using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HandleState
{
    public class InputState{
        public int tick;
        public Vector3 moveVector;
    }
    public class TransformStateRW : INetworkSerializable{
        public int tick;
        public Vector3 finalPos;
        public Vector3 finalVelocity;
        public Vector3 finalAngularVelocity;
        public Quaternion finalRotation;
        public bool isMoving;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T: IReaderWriter{
            if(serializer.IsReader){
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out tick);
                reader.ReadValueSafe(out finalPos);
                reader.ReadValueSafe(out finalRotation);
                reader.ReadValueSafe(out isMoving);

            }
            else{
                var writer = serializer.GetFastBufferWriter();
                writer.WriteValueSafe(tick);
                writer.WriteValueSafe(finalPos);
                writer.WriteValueSafe(finalRotation);
                writer.WriteValueSafe(isMoving);
            }
        }
    }
}
