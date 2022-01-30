using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class SongFileRW
{ 
    public void WriteSongFile(SpectralFluxAnalyzer analyzer, string title, float length)
    {
        /* To put in song file:
         * Song length?
         * Each of the onsets (time, lane)
         */
        string path = "Assets/Songs/" + title + ".txt";
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(length.ToString());

        //For each sample, find the highest peaking frequency bin (if there is one) and write it to the file
        int numSamples = analyzer.subSpectralFluxSamples[0].Count;
        int numBins = analyzer.subSpectralFluxSamples.Count;
        for(int sampleNum = 0; sampleNum < numSamples; ++sampleNum)
        {
            float peakFlux = 0f;
            int peakBin = -1;
            float time = analyzer.subSpectralFluxSamples[0][sampleNum].time;
            for(int bin = 0; bin < numBins; ++bin)
            {
                SpectralFluxInfo sample = analyzer.subSpectralFluxSamples[bin][sampleNum];
                if(sample.isPeak && sample.prunedSpectralFlux > peakFlux)
                {
                    peakFlux = sample.prunedSpectralFlux;
                    peakBin = bin;
                }
            }
            if(peakBin != -1)
            {
                writer.WriteLine(time.ToString() + " " + peakBin.ToString());
            }
        }
        writer.Close();
    }

    public List<Tuple<float, int>> ReadSongFile(string file)
    {
        List<Tuple<float, int>> data = new List<Tuple<float, int>>();
        string[] lines = System.IO.File.ReadAllLines(file);
        //Ignore first line (length of song) for now
        for(int i = 1; i < lines.Length; ++i)
        {
            string[] elements = lines[i].Split(' ');
            Tuple<float, int> dataPiece = new Tuple<float, int>((float)Convert.ToDouble(elements[0]), Convert.ToInt32(elements[1]));
            data.Add(dataPiece);
        }
        return data;
    }
}
