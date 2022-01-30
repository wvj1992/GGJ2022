//Functionality from https://medium.com/giant-scam/algorithmic-beat-mapping-in-unity-preprocessed-audio-analysis-d41c339c135a
//Github Repo where code samples are pulled from: https://github.com/jesse-scam/algorithmic-beat-mapping-unity

using UnityEngine;
using System;
using System.Numerics;
using DSPLib;
using System.Threading;
using System.Collections.Generic;

public class AudioPreprocessor : MonoBehaviour
{
    private AudioSource aud;
    private float[] multiChannelSamples;
    private int numChannels;
    private int numTotalSamples;
    private float clipLength;
    private int sampleRate;
    private string title;
    private SpectralFluxAnalyzer preProcessedSpectralFluxAnalyzer;
    private List<Tuple<int, int>> rangesOfInterest;
    private SongFileRW fileWriter;
    //PlotController preProcessedPlotController;


    // Start is called before the first frame update
    void Start()
    {
        //Init all the necessary data we need for the threaded FFT work
        init();

        //Begin thread to preprocess audio
        Thread bgThread = new Thread(this.getFullSpectrumThreaded);
        Debug.Log("Starting Background Thread");
        bgThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Inits all member variables
    private void init()
    {
        preProcessedSpectralFluxAnalyzer = new SpectralFluxAnalyzer();
        fileWriter = new SongFileRW();
        //preProcessedPlotController = GameObject.Find("PreprocessedPlot").GetComponent<PlotController>();

        aud = GetComponent<AudioSource>();
        multiChannelSamples = new float[aud.clip.samples * aud.clip.channels];
        numChannels = aud.clip.channels;
        numTotalSamples = aud.clip.samples;
        clipLength = aud.clip.length;
        sampleRate = aud.clip.frequency;
        title = aud.clip.name;
        Debug.Log("Song name is " + title);

        //Ranges of interest in order: Sub Bass, Bass, Low Midrange, Midrange, Upper Midrange, Presence, Brilliance
        rangesOfInterest = new List<Tuple<int, int>>();
        rangesOfInterest.Add(new Tuple<int, int>(20, 60));
        rangesOfInterest.Add(new Tuple<int, int>(60, 250));
        rangesOfInterest.Add(new Tuple<int, int>(250, 500));
        rangesOfInterest.Add(new Tuple<int, int>(500, 2000));
        rangesOfInterest.Add(new Tuple<int, int>(2000, 4000));
        rangesOfInterest.Add(new Tuple<int, int>(4000, 5000));
        rangesOfInterest.Add(new Tuple<int, int>(6000, 20000));


        aud.clip.GetData(multiChannelSamples, 0);
        Debug.Log("GetData done");
    }

    //Combines the samples into one channel
    private void CombineChannels(float[] samples)
    {
        int numProcessed = 0;
        float combinedChannelAverage = 0f;
        for (int i = 0; i < multiChannelSamples.Length; ++i)
        {
            combinedChannelAverage += multiChannelSamples[i];

            // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
            if ((i + 1) % this.numChannels == 0)
            {
                samples[numProcessed] = combinedChannelAverage / this.numChannels;
                numProcessed++;
                combinedChannelAverage = 0f;
            }
        }
    }

    public void getFullSpectrumThreaded()
    {
        try
        {
            // We only need to retain the samples for combined channels over the time domain
            float[] preProcessedSamples = new float[this.numTotalSamples];
            CombineChannels(preProcessedSamples);
            Debug.Log("Combine Channels done");
            Debug.Log(preProcessedSamples.Length);

            DoFFT(preProcessedSamples);
            Debug.Log("Spectrum Analysis done");

            //TODO: Output info to file
            fileWriter.WriteSongFile(preProcessedSpectralFluxAnalyzer, title, clipLength);
            Debug.Log("File writing done");
            Debug.Log("Background Thread Completed");

        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    //Completes a Fast Fourier Transform (FFT) on a given array of samples
    private void DoFFT(float[] samples)
    {
        // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
        int spectrumSampleSize = 1024;
        int iterations = samples.Length / spectrumSampleSize;
        Tuple<int, int>[] bins = calculateBins(spectrumSampleSize);

        FFT fft = new FFT();
        fft.Initialize((UInt32)spectrumSampleSize);

        Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
        double[] sampleChunk = new double[spectrumSampleSize];
        for (int i = 0; i < iterations; i++)
        {
            // Grab the current 1024 chunk of audio sample data
            Array.Copy(samples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

            // Apply our chosen FFT Window
            double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
            double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
            double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            // Perform the FFT and convert output (complex numbers) to Magnitude
            Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
            double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
            scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

            // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
            float curSongTime = GetTimeFromIndex(i) * spectrumSampleSize;

            // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
            preProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime, bins);
        }
    }

    private int GetIndexFromTime(float curTime)
    {
        float lengthPerSample = this.clipLength / (float)this.numTotalSamples;

        return Mathf.FloorToInt(curTime / lengthPerSample);
    }

    private float GetTimeFromIndex(int index)
    {
        return ((1f / (float)this.sampleRate) * index);
    }

    //Calculates the bounds for given frequency ranges of interest
    private Tuple<int, int>[] calculateBins(int spectrumSampleSize)
    {
        Tuple<int, int>[] bins = new Tuple<int, int>[rangesOfInterest.Count];

        //FFT was fed in spectrumSampleSize audio samples, resulting in spectrumSampleSize/2 spectrum values
        float numSpectrumValues = spectrumSampleSize / 2;

        //Supported frequency range after FFT is sampleRate/2. Divide this by num of spectrum values from FFT for Hz/bin value
        float binSize = sampleRate / 2 / numSpectrumValues;

        int binIndex = 0;
        foreach(Tuple<int, int> range in rangesOfInterest)
        {
            int lowerBound = (int)Math.Floor(range.Item1 / binSize);
            int higherBound = (int)Math.Floor(range.Item2 / binSize);
            bins[binIndex] = new Tuple<int, int>(lowerBound, higherBound);
            binIndex++;
            Debug.Log(string.Format("For bin {0} with frequencies {1} - {2}:\n"
                + "Lower Bound Index: {3}\n"
                + "Higher Bound Index: {4}", binIndex, range.Item1, range.Item2, lowerBound, higherBound));
        }
        return bins;
    }

}
