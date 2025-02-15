﻿using System;
using ManagedBass;
using T3.Core.IO;
using T3.Core.Resource;
using T3.Core.Utils;

namespace T3.Core.Audio
{
    /// <summary>
    /// Analyze audio input from internal soundtrack or from selected Wasapi device.
    /// Transforms the FFT buffer generated by BASS and generates frequency bands and peaks.
    /// </summary>
    public static class AudioAnalysis
    {
        public enum InputModes
        {
            Soundtrack,
            WasapiDevice,
        }

        public static void CompleteFrame()
        {
            if (InputMode == InputModes.WasapiDevice)
            {
                WasapiAudioInput.CompleteFrame();
            }
            
            var lastTargetIndex = -1;

            for (var binIndex = 0; binIndex < FftHalfSize; binIndex++)
            {
                var gain = FftGainBuffer[binIndex];
                var gainDb = (gain <= 0.000001f) ? float.NegativeInfinity : (20 * MathF.Log10(gain));

                var normalizedValue = MathUtils.RemapAndClamp(gainDb, -80, 0, 0, 1);
                FftNormalizedBuffer[binIndex] = normalizedValue ;
                
                var bandIndex = _bandIndexForFftBinIndices[binIndex];
                if (bandIndex == NoBandIndex)
                    continue;
                
                if (bandIndex != lastTargetIndex)
                {
                    FrequencyBands[bandIndex] = 0;
                    lastTargetIndex = bandIndex;
                }
                
                FrequencyBands[bandIndex] = MathF.Max(FrequencyBands[bandIndex], normalizedValue);
            }
            
            // Update Peaks
            for (var bandIndex = 0; bandIndex < FrequencyBandCount; bandIndex++)
            {
                var lastPeak = FrequencyBandPeaks[bandIndex];
                var decayed = lastPeak * ProjectSettings.Config.AudioDecayFactor;
                var currentValue = FrequencyBands[bandIndex];
                var newPeak = MathF.Max(decayed, currentValue);
                FrequencyBandPeaks[bandIndex] = newPeak;

                const float attackAmplification = 4;
                var newAttack = (newPeak - lastPeak).Clamp(0, 1000) * attackAmplification;
                var lastAttackDecayed = FrequencyBandAttacks[bandIndex] * ProjectSettings.Config.AudioDecayFactor; 
                FrequencyBandAttacks[bandIndex] = MathF.Max(newAttack, lastAttackDecayed);
                FrequencyBandAttackPeaks[bandIndex] = MathF.Max(FrequencyBandAttackPeaks[bandIndex]*0.995f, FrequencyBandAttacks[bandIndex]);
            }
        }

        private static int[] InitializeBandsLookupsTable()
        {
            var r = new int[FftHalfSize];
            const float lowestBandFrequency = 55; 
            const float highestBandFrequency = 15000;
                
            var maxOctave = MathF.Log2(highestBandFrequency / lowestBandFrequency); 
            for (var i = 0; i < FftHalfSize; i++)
            {
                var bandIndex = NoBandIndex;
                var freq = ((float)i / FftHalfSize) * (48000f / 2f);

                switch (i)
                {
                    case 0:
                        break;
                    
                    // For low frequency bin we fake a direct mapping to avoid gaps
                    case < 6:
                        bandIndex = i - 1;
                        break;
                    default:
                    {
                        var octave = MathF.Log2(freq / lowestBandFrequency);
                        var octaveNormalized = (octave) / (maxOctave);
                        bandIndex = (int)(octaveNormalized * FrequencyBandCount);
                        if (bandIndex >= FrequencyBandCount)
                            bandIndex = NoBandIndex;
                        break;
                    }
                }
                //Log.Warning($" #{i}  band {bandIndex}  {freq}hz");
                r[i] = bandIndex;
            }

            return r;
        }
        
        private static int[] _bandIndexForFftBinIndices = InitializeBandsLookupsTable();
        private const int NoBandIndex = -1;
 
        public static InputModes InputMode = InputModes.Soundtrack;

        public const int FrequencyBandCount = 32;
        public static readonly float[] FrequencyBands = new float[FrequencyBandCount];
        public static readonly float[] FrequencyBandPeaks = new float[FrequencyBandCount];
        
        public static readonly float[] FrequencyBandAttacks = new float[FrequencyBandCount];
        public static readonly float[] FrequencyBandAttackPeaks = new float[FrequencyBandCount];
        
        /// <summary>
        /// Result of the fft analysis in gain
        /// </summary>
        public static readonly float[] FftGainBuffer = new float[FftHalfSize];
        
        /// <summary>
        /// Result of the fft analysis converted to db and mapped to a normalized range   
        /// </summary>
        public static readonly float[] FftNormalizedBuffer = new float[FftHalfSize];
        
        public const DataFlags BassFlagForFftBufferSize = DataFlags.FFT2048;
        public const int FftHalfSize = 1024; // Half the fft resolution

        public static float AudioLevel;
    }
}