using Newtonsoft.Json;
using System.Runtime.Serialization;

public class RandomState
{
    [JsonIgnore]
    public System.Random Random;

    public int seed;
    public int calls;

    public RandomState(int seed, int calls = 0)
    {
        this.seed = seed;
        this.calls = calls;
        RebuildRandom();
    }

    public RandomState Clone()
    {
        return new RandomState(this.seed, this.calls);
    }

    public void CopyFrom(RandomState other)
    {
        seed = other.seed;
        calls = other.calls;
        RebuildRandom();
    }
    
    

    [JsonConstructor]
    public RandomState()
    {
    }

    public void RebuildRandom()
    {
        Random = new System.Random(seed);

        for (int i = 0; i < calls; i++)
        {
            Random.Next();
        }
    }

    [OnDeserialized]
    internal void OnDeserializedMethod(StreamingContext context)
    {
        RebuildRandom();
    }

    public int Next()
    {
        if (Random == null)
        {
            RebuildRandom();
        }
        
        calls++;
        return Random.Next();
    }

    public int Next(int maxValue)
    {
        if (Random == null)
        {
            RebuildRandom();
        }
        
        calls++;
        return Random.Next(maxValue);
    }

    public int Next(int minValue, int maxValue)
    {
        if (Random == null)
        {
            RebuildRandom();
        }
        
        calls++;
        return Random.Next(minValue, maxValue);
    }

    public double NextDouble()
    {
        if (Random == null)
        {
            RebuildRandom();
        }
        
        calls++;
        return Random.NextDouble();
    }

    public void NextBytes(byte[] bytes)
    {
        if (Random == null)
        {
            RebuildRandom();
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = (byte)Next(0, 256);
        }
    }
}
