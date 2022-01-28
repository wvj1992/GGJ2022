//Functionality from https://medium.com/giant-scam/algorithmic-beat-mapping-in-unity-preprocessed-audio-analysis-d41c339c135a
//Github Repo where code samples are pulled from: https://github.com/jesse-scam/algorithmic-beat-mapping-unity

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Numerics;
using DSPLib;

public class AudioPreprocessor : MonoBehaviour
{
    private AudioSource aud;
    private float[] multiChannelSamples;
    private int numChannels;
    private int numTotalSamples;
    private float clipLength;
    private float sampleRate;

    // Start is called before the first frame update
    void Start()
    {
        init();
        //Combine channels
        float[] preProcessedSamples = new float[this.numTotalSamples];
        combineChannels(preProcessedSamples);

        //TODO: Replace "combine channels" step with getFullSpectrumThreaded using threads
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Inits all member variables
    private void init()
    {
        aud = GetComponent<AudioSource>();
        multiChannelSamples = new float[aud.clip.samples * aud.clip.channels];
        numChannels = aud.clip.channels;
        numTotalSamples = aud.clip.samples;
        clipLength = aud.clip.length;
        sampleRate = aud.clip.frequency;

        aud.clip.GetData(multiChannelSamples, 0);
    }

    //Combines the samples into one channel
    private void combineChannels(float[] samples)
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
            combineChannels(preProcessedSamples);
            //int numProcessed = 0;
            //float combinedChannelAverage = 0f;
            //for (int i = 0; i < multiChannelSamples.Length; i++)
            //{
            //    combinedChannelAverage += multiChannelSamples[i];

            //    // Each time we have processed all channels samples for a point in time, we will store the average of the channels combined
            //    if ((i + 1) % this.numChannels == 0)
            //    {
            //        preProcessedSamples[numProcessed] = combinedChannelAverage / this.numChannels;
            //        numProcessed++;
            //        combinedChannelAverage = 0f;
            //    }
            //}

            Debug.Log("Combine Channels done");
            Debug.Log(preProcessedSamples.Length);

            doFFT(preProcessedSamples);
            //// Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
            //int spectrumSampleSize = 1024;
            //int iterations = preProcessedSamples.Length / spectrumSampleSize;

            //FFT fft = new FFT();
            //fft.Initialize((UInt32)spectrumSampleSize);

            //Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
            //double[] sampleChunk = new double[spectrumSampleSize];
            //for (int i = 0; i < iterations; i++)
            //{
            //    // Grab the current 1024 chunk of audio sample data
            //    Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

            //    // Apply our chosen FFT Window
            //    double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
            //    double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
            //    double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            //    // Perform the FFT and convert output (complex numbers) to Magnitude
            //    Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
            //    double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
            //    scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

            //    // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
            //    float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

            //    // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
            //    preProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
            //}

            Debug.Log("Spectrum Analysis done");
            Debug.Log("Background Thread Completed");

        }
        catch (Exception e)
        {
            // Catch exceptions here since the background thread won't always surface the exception to the main thread
            Debug.Log(e.ToString());
        }
    }

    //Completes a Fast Fourier Transform (FFT) on a given array of 
    private void doFFT(float[] samples)
    {
        // Once we have our audio sample data prepared, we can execute an FFT to return the spectrum data over the time domain
        int spectrumSampleSize = 1024;
        int iterations = samples.Length / spectrumSampleSize;

        FFT fft = new FFT();
        fft.Initialize((UInt32)spectrumSampleSize);

        Debug.Log(string.Format("Processing {0} time domain samples for FFT", iterations));
        double[] sampleChunk = new double[spectrumSampleSize];
        for (int i = 0; i < iterations; i++)
        {
            // Grab the current 1024 chunk of audio sample data
            Array.Copy(preProcessedSamples, i * spectrumSampleSize, sampleChunk, 0, spectrumSampleSize);

            // Apply our chosen FFT Window
            double[] windowCoefs = DSP.Window.Coefficients(DSP.Window.Type.Hanning, (uint)spectrumSampleSize);
            double[] scaledSpectrumChunk = DSP.Math.Multiply(sampleChunk, windowCoefs);
            double scaleFactor = DSP.Window.ScaleFactor.Signal(windowCoefs);

            // Perform the FFT and convert output (complex numbers) to Magnitude
            Complex[] fftSpectrum = fft.Execute(scaledSpectrumChunk);
            double[] scaledFFTSpectrum = DSPLib.DSP.ConvertComplex.ToMagnitude(fftSpectrum);
            scaledFFTSpectrum = DSP.Math.Multiply(scaledFFTSpectrum, scaleFactor);

            // These 1024 magnitude values correspond (roughly) to a single point in the audio timeline
            float curSongTime = getTimeFromIndex(i) * spectrumSampleSize;

            // Send our magnitude data off to our Spectral Flux Analyzer to be analyzed for peaks
            preProcessedSpectralFluxAnalyzer.analyzeSpectrum(Array.ConvertAll(scaledFFTSpectrum, x => (float)x), curSongTime);
        }
    }
}
