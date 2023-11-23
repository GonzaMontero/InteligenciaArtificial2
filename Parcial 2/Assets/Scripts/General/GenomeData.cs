[System.Serializable]
public class GenomeData
{
    public float mutationChance = 0.10f;
    public float mutationRate = 0.01f;
    
    public float[] genome;
    public int populationCount;
    public int inputsCount;
    public int hiddenLayers;
    public int outputsCount;
    public int neuronCountPerHiddenLayer;
    public float bias;
    public float sigmoid;

    public GenomeData()
    {
        
    }

    public GenomeData(GenomeData copy)
    {
        mutationChance = copy.mutationChance;
        mutationRate = copy.mutationRate;

        if (copy.genome is { Length: > 0 })
        {
            genome = new float[copy.genome.Length];
            for (int i = 0; i < copy.genome.Length; i++)
            {
                genome[i] = copy.genome[i];
            }
        }
        
        populationCount = copy.populationCount;
        inputsCount = copy.inputsCount;
        hiddenLayers = copy.hiddenLayers;
        outputsCount = copy.outputsCount;
        neuronCountPerHiddenLayer = copy.neuronCountPerHiddenLayer;
        bias = copy.bias;
        sigmoid = copy.sigmoid;
    }
    
}
