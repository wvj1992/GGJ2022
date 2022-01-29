using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectralFluxInfo
{
	public float time;
	public float spectralFlux;
	public float threshold;
	public float prunedSpectralFlux;
	public bool isPeak;
}

public class SpectralFluxAnalyzer
{
	int numSamples = 1024;

	// Sensitivity multiplier to scale the average threshold.
	// In this case, if a rectified spectral flux sample is > 1.5 times the average, it is a peak
	float thresholdMultiplier = 1.5f;

	// Number of samples to average in our window
	int thresholdWindowSize = 50;

	public List<SpectralFluxInfo> spectralFluxSamples;

	public List<List<SpectralFluxInfo>> subSpectralFluxSamples;

	float[] curSpectrum;
	float[] prevSpectrum;

	int indexToProcess;

	public SpectralFluxAnalyzer()
	{
		spectralFluxSamples = new List<SpectralFluxInfo>();

		subSpectralFluxSamples = new List<List<SpectralFluxInfo>>();

		// Start processing from middle of first window and increment by 1 from there
		indexToProcess = thresholdWindowSize / 2;

		curSpectrum = new float[numSamples];
		prevSpectrum = new float[numSamples];
	}

	public void setCurSpectrum(float[] spectrum)
	{
		curSpectrum.CopyTo(prevSpectrum, 0);
		spectrum.CopyTo(curSpectrum, 0);
	}

	public void analyzeSpectrum(float[] spectrum, float time)
	{
		// Set spectrum
		setCurSpectrum(spectrum);

		// Get current spectral flux from spectrum
		SpectralFluxInfo curInfo = new SpectralFluxInfo();
		curInfo.time = time;
		curInfo.spectralFlux = calculateRectifiedSpectralFlux();
		spectralFluxSamples.Add(curInfo);

		// We have enough samples to detect a peak
		if (spectralFluxSamples.Count >= thresholdWindowSize)
		{
			// Get Flux threshold of time window surrounding index to process
			spectralFluxSamples[indexToProcess].threshold = getFluxThreshold(indexToProcess);

			// Only keep amp amount above threshold to allow peak filtering
			spectralFluxSamples[indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess);

			// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
			int indexToDetectPeak = indexToProcess - 1;

			bool curPeak = isPeak(indexToDetectPeak);

			if (curPeak)
			{
				spectralFluxSamples[indexToDetectPeak].isPeak = true;
				logSample(indexToDetectPeak);
			}
			indexToProcess++;
		}
		else
		{
			Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", spectralFluxSamples.Count, thresholdWindowSize));
		}
	}

	//TODO: Condense code
	public void analyzeSpectrum(float[] spectrum, float time, Tuple<int, int>[] bins)
    {
		setCurSpectrum(spectrum);
		for(int i = 0; i < bins.Length; ++i)
        {
			//First time passing through, we initialize the list of spectral flux info for each spectrum bin
			if(subSpectralFluxSamples.Count == i)
            {
				subSpectralFluxSamples.Add(new List<SpectralFluxInfo>());
				//Debug.Log(string.Format("Creating new subSpectralFluxSample list for bin {0}", i));
            }
			// Get current spectral flux from spectrum
			SpectralFluxInfo curInfo = new SpectralFluxInfo();
			curInfo.time = time;
			curInfo.spectralFlux = calculateRectifiedSpectralFlux(bins[i]);
			subSpectralFluxSamples[i].Add(curInfo);

			// We have enough samples within a bin to detect a peak
			if (subSpectralFluxSamples[i].Count >= thresholdWindowSize)
			{
				//Debug.Log(string.Format("Attempting to detect peak in bin {0}. Index to process is {1}", i, indexToProcess));
				//Debug.Log(string.Format("Length of list of bins' samples is {0}. Length of this bin's sample list is {1}", subSpectralFluxSamples.Count, subSpectralFluxSamples[i].Count));

				// Get Flux threshold of time window surrounding index to process
				//TODO: This might be the same for all bins at a given index because we're getting the average of all samples from the bins we're looking at
				subSpectralFluxSamples[i][indexToProcess].threshold = getFluxThreshold(indexToProcess, i);

				// Only keep amp amount above threshold to allow peak filtering
				subSpectralFluxSamples[i][indexToProcess].prunedSpectralFlux = getPrunedSpectralFlux(indexToProcess, i);

				// Now that we are processed at n, n-1 has neighbors (n-2, n) to determine peak
				int indexToDetectPeak = indexToProcess - 1;

				bool curPeak = isPeak(indexToDetectPeak, i);

				if (curPeak)
				{
					subSpectralFluxSamples[i][indexToDetectPeak].isPeak = true;
					logSubSample(indexToDetectPeak, i);
				}
				if(i == bins.Length - 1)
                {
					indexToProcess++;
				}
			}
			else
			{
				Debug.Log(string.Format("Not ready yet.  At spectral flux sample size of {0} growing to {1}", subSpectralFluxSamples[i].Count, thresholdWindowSize));
			}
		}
    }

	float calculateRectifiedSpectralFlux(Tuple<int, int> bin = null)
	{
		float sum = 0f;
		bool checkingBin = (bin != null);
		int lower = (checkingBin) ? bin.Item1 : 0;
		int higher = (checkingBin) ? bin.Item2 + 1 : numSamples;
		// Aggregate positive changes in spectrum data
		for (int i = lower; i < higher; i++)
		{
			sum += Mathf.Max(0f, curSpectrum[i] - prevSpectrum[i]);
		}
		return sum;
	}

	float getFluxThreshold(int spectralFluxIndex, int binIndex = -1)
	{
		// How many samples in the past and future we include in our average
		int windowStartIndex = Mathf.Max(0, spectralFluxIndex - thresholdWindowSize / 2);
		int windowEndIndex = 0;
		if(binIndex != -1)
        {
			windowEndIndex = Mathf.Min(subSpectralFluxSamples[binIndex].Count - 1, spectralFluxIndex + thresholdWindowSize / 2);
		}
		else
		{
			windowEndIndex = Mathf.Min(spectralFluxSamples.Count - 1, spectralFluxIndex + thresholdWindowSize / 2);
		}

		// Add up our spectral (or sub spectral) flux over the window
		float sum = 0f;
		for (int i = windowStartIndex; i < windowEndIndex; i++)
		{
			if(binIndex != -1)
            {
				for(int j = 0; j < subSpectralFluxSamples.Count; j++)
                {
					sum += subSpectralFluxSamples[j][i].spectralFlux;
				}
            }
			else
            {
				sum += spectralFluxSamples[i].spectralFlux;
			}
		}

		// Return the average multiplied by our sensitivity multiplier
		float avg = sum / (windowEndIndex - windowStartIndex);
		return avg * thresholdMultiplier;
	}

	float getPrunedSpectralFlux(int spectralFluxIndex, int binIndex = -1)
	{
		if(binIndex != -1)
        {
			return Mathf.Max(0f, subSpectralFluxSamples[binIndex][spectralFluxIndex].spectralFlux - subSpectralFluxSamples[binIndex][spectralFluxIndex].threshold);

		}
		else
        {
			return Mathf.Max(0f, spectralFluxSamples[spectralFluxIndex].spectralFlux - spectralFluxSamples[spectralFluxIndex].threshold);
		}
	}

	bool isPeak(int spectralFluxIndex, int binIndex = -1)
	{
		if (binIndex != -1)
		{
			if (subSpectralFluxSamples[binIndex][spectralFluxIndex].prunedSpectralFlux > subSpectralFluxSamples[binIndex][spectralFluxIndex + 1].prunedSpectralFlux &&
				subSpectralFluxSamples[binIndex][spectralFluxIndex].prunedSpectralFlux > subSpectralFluxSamples[binIndex][spectralFluxIndex - 1].prunedSpectralFlux)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			if (spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex + 1].prunedSpectralFlux &&
				spectralFluxSamples[spectralFluxIndex].prunedSpectralFlux > spectralFluxSamples[spectralFluxIndex - 1].prunedSpectralFlux)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	void logSample(int indexToLog)
	{
		int windowStart = Mathf.Max(0, indexToLog - thresholdWindowSize / 2);
		int windowEnd = Mathf.Min(spectralFluxSamples.Count - 1, indexToLog + thresholdWindowSize / 2);
		Debug.Log(string.Format(
			"Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
			"Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.",
			spectralFluxSamples[indexToLog].time,
			spectralFluxSamples[indexToLog].prunedSpectralFlux,
			spectralFluxSamples[indexToLog].spectralFlux,
			spectralFluxSamples[indexToLog].threshold,
			spectralFluxSamples[windowStart].time,
			spectralFluxSamples[windowEnd].time,
			spectralFluxSamples[windowEnd].time - spectralFluxSamples[windowStart].time,
			windowEnd - windowStart
		));
	}

	//TODO: Add interesting factoids about which bin we found the peak in
	void logSubSample(int indexToLog, int bin)
	{
		int windowStart = Mathf.Max(0, indexToLog - thresholdWindowSize / 2);
		int windowEnd = Mathf.Min(subSpectralFluxSamples[bin].Count - 1, indexToLog + thresholdWindowSize / 2);
		Debug.Log(string.Format(
			"Peak detected at song time {0} with pruned flux of {1} ({2} over thresh of {3}).\n" +
			"Thresh calculated on time window of {4}-{5} ({6} seconds) containing {7} samples.\n" +
			"Found in bin {8}.",
			subSpectralFluxSamples[bin][indexToLog].time,
			subSpectralFluxSamples[bin][indexToLog].prunedSpectralFlux,
			subSpectralFluxSamples[bin][indexToLog].spectralFlux,
			subSpectralFluxSamples[bin][indexToLog].threshold,
			subSpectralFluxSamples[bin][windowStart].time,
			subSpectralFluxSamples[bin][windowEnd].time,
			subSpectralFluxSamples[bin][windowEnd].time - subSpectralFluxSamples[bin][windowStart].time,
			windowEnd - windowStart,
			bin
		));
	}
}