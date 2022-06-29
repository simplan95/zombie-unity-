using ExitGames.Client.Photon;
using UnityEngine;

//컬러정보 교환시 SerializeColor을 통해 컬러정보를 바이트화 해서 보내고 컬러정보를 받을 시 DeserializeColor을 통해 컬러정보를 받는다
public class ColorSerialization 
{
    private static byte[] colorMemory = new byte[4 * 4];

    public static short SerializeColor(StreamBuffer outStream, object targetObject) 
    {
        //컬러인스턴스 생성
        Color color = (Color) targetObject;

        lock (colorMemory)
        {
            byte[] bytes = colorMemory;
            int index = 0;
            
            Protocol.Serialize(color.r, bytes, ref index);
            Protocol.Serialize(color.g, bytes, ref index);
            Protocol.Serialize(color.b, bytes, ref index);
            Protocol.Serialize(color.a, bytes, ref index);
            outStream.Write(bytes, 0, 4*4);
        }

        return 4 * 4;
    }

    public static object DeserializeColor(StreamBuffer inStream, short length)  
    {
        Color color = new Color();
  
        lock (colorMemory)
        {
            inStream.Read(colorMemory, 0, 4 * 4);
            int index = 0;
            
            Protocol.Deserialize(out color.r,colorMemory, ref index);
            Protocol.Deserialize(out color.g,colorMemory, ref index);
            Protocol.Deserialize(out color.b,colorMemory, ref index);
            Protocol.Deserialize(out color.a,colorMemory, ref index);
        }
        
        return color;
    }
}