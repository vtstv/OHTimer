// Copyright (c) Murr (https://github.com/vtstv). All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using OnceHumanEventTimer.Core.Models;

namespace OnceHumanEventTimer.Infrastructure.Audio;

/// <summary>
/// Procedural PCM waveform generator for in-memory sci-fi chimes and alerts.
/// </summary>
public static class AudioWaveformGenerator
{
    private const int SampleRate = 44100;

    /// <summary>
    /// Generates a valid 16-bit mono 44.1kHz PCM WAV stream for the specified sound preset.
    /// </summary>
    public static MemoryStream GeneratePresetWav(SoundPreset preset, double volume)
    {
        volume = Math.Clamp(volume, 0.0, 1.0);

        return preset switch
        {
            SoundPreset.CelestialBell => GenerateCelestialBell(volume),
            SoundPreset.CosmicPulse => GenerateCosmicPulse(volume),
            SoundPreset.StardustPing => GenerateStardustPing(volume),
            SoundPreset.QuantumChord => GenerateQuantumChord(volume),
            SoundPreset.DeepResonance => GenerateDeepResonance(volume),
            SoundPreset.CyberWarning => GenerateCyberWarning(volume),
            SoundPreset.EmergencySiren => GenerateEmergencySiren(volume),
            _ => GenerateCosmicPulse(volume)
        };
    }

    private static MemoryStream GenerateCelestialBell(double volume)
    {
        // Bright cosmic bell arpeggio: Eb5 -> G5 -> Bb5 -> Eb6
        double[] freqs = [622.25, 783.99, 932.33, 1244.51];
        double[] durations = [0.10, 0.10, 0.12, 0.40];
        return SynthesizeSequentialTones(freqs, durations, volume, decayMultiplier: 3.5, overtoneLevel: 0.35);
    }

    private static MemoryStream GenerateCosmicPulse(double volume)
    {
        // Gentle two-tone sci-fi beacon: 660Hz -> 880Hz
        double[] freqs = [660.0, 880.0];
        double[] durations = [0.15, 0.25];
        return SynthesizeSequentialTones(freqs, durations, volume, decayMultiplier: 3.0, overtoneLevel: 0.20);
    }

    private static MemoryStream GenerateStardustPing(double volume)
    {
        // Crisp crystal sonar ping: high tone with shimmering resonance
        int totalSamples = (int)(SampleRate * 0.40);
        short[] samples = new short[totalSamples];
        double fundamental = 1760.0; // A6
        double harmonic = 2640.0;    // E7

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            double attack = Math.Min(1.0, t / 0.005);
            double decay = Math.Exp(-7.0 * t);
            double signal = (Math.Sin(2.0 * Math.PI * fundamental * t) * 0.75 +
                             Math.Sin(2.0 * Math.PI * harmonic * t) * 0.25) * attack * decay * volume;

            samples[i] = (short)Math.Clamp(signal * short.MaxValue * 0.75, short.MinValue, short.MaxValue);
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream GenerateQuantumChord(double volume)
    {
        // Harmonious polyphonic chord chime: F5, A5, C6, E6 played simultaneously
        int totalSamples = (int)(SampleRate * 0.55);
        short[] samples = new short[totalSamples];
        double[] chord = [698.46, 880.00, 1046.50, 1318.51];

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            double attack = Math.Min(1.0, t / 0.02);
            double decay = Math.Exp(-3.2 * t);
            double waveSum = 0;
            foreach (double f in chord)
            {
                waveSum += Math.Sin(2.0 * Math.PI * f * t);
            }
            double signal = (waveSum / chord.Length) * attack * decay * volume;
            samples[i] = (short)Math.Clamp(signal * short.MaxValue * 0.75, short.MinValue, short.MaxValue);
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream GenerateDeepResonance(double volume)
    {
        // Low bass harmonic gong: 110Hz fundamental with warm decaying overtones
        int totalSamples = (int)(SampleRate * 0.65);
        short[] samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            double attack = Math.Min(1.0, t / 0.03);
            double decay = Math.Exp(-2.8 * t);
            double signal = (Math.Sin(2.0 * Math.PI * 110.0 * t) * 0.60 +
                             Math.Sin(2.0 * Math.PI * 220.0 * t) * 0.25 +
                             Math.Sin(2.0 * Math.PI * 330.0 * t) * 0.15) * attack * decay * volume;

            samples[i] = (short)Math.Clamp(signal * short.MaxValue * 0.85, short.MinValue, short.MaxValue);
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream GenerateCyberWarning(double volume)
    {
        // Fast high-tech electronic alert: rapid staccato beeps (950Hz, 1250Hz, 950Hz, 1250Hz)
        double[] freqs = [950.0, 1250.0, 950.0, 1250.0];
        double[] durations = [0.06, 0.06, 0.06, 0.10];

        int totalSamples = 0;
        for (int i = 0; i < durations.Length; i++) totalSamples += (int)(SampleRate * durations[i]);
        short[] samples = new short[totalSamples];
        int sampleIndex = 0;

        for (int note = 0; note < freqs.Length; note++)
        {
            double f = freqs[note];
            int noteSamples = (int)(SampleRate * durations[note]);
            for (int i = 0; i < noteSamples; i++)
            {
                double t = (double)i / SampleRate;
                double decay = Math.Exp(-2.0 * t / durations[note]);
                // Sine + slight digital square overtone
                double sine = Math.Sin(2.0 * Math.PI * f * t);
                double square = Math.Sign(sine) * 0.2;
                double signal = (sine * 0.8 + square) * decay * volume;
                samples[sampleIndex++] = (short)Math.Clamp(signal * short.MaxValue * 0.70, short.MinValue, short.MaxValue);
            }
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream GenerateEmergencySiren(double volume)
    {
        // Urgent pulsating warble: frequency modulated between 600Hz and 1100Hz
        int totalSamples = (int)(SampleRate * 0.50);
        short[] samples = new short[totalSamples];
        double phase = 0;

        for (int i = 0; i < totalSamples; i++)
        {
            double t = (double)i / SampleRate;
            double currentFreq = 850.0 + 250.0 * Math.Sin(2.0 * Math.PI * 8.0 * t);
            phase += 2.0 * Math.PI * currentFreq / SampleRate;
            double envelope = Math.Min(1.0, t / 0.02) * (1.0 - (t / 0.55));
            double signal = Math.Sin(phase) * Math.Max(0.0, envelope) * volume;
            samples[i] = (short)Math.Clamp(signal * short.MaxValue * 0.75, short.MinValue, short.MaxValue);
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream SynthesizeSequentialTones(
        double[] frequencies,
        double[] durations,
        double volume,
        double decayMultiplier,
        double overtoneLevel)
    {
        int totalSamples = 0;
        for (int i = 0; i < durations.Length; i++) totalSamples += (int)(SampleRate * durations[i]);

        short[] samples = new short[totalSamples];
        int sampleIndex = 0;

        for (int note = 0; note < frequencies.Length; note++)
        {
            double freq = frequencies[note];
            int noteSamples = (int)(SampleRate * durations[note]);

            for (int i = 0; i < noteSamples; i++)
            {
                double t = (double)i / SampleRate;
                double envelope = Math.Exp(-decayMultiplier * t / durations[note]);
                double fundamental = Math.Sin(2.0 * Math.PI * freq * t);
                double overtone = overtoneLevel * Math.Sin(4.0 * Math.PI * freq * t);
                double signal = (fundamental + overtone) * envelope * volume;

                samples[sampleIndex++] = (short)Math.Clamp(signal * short.MaxValue * 0.70, short.MinValue, short.MaxValue);
            }
        }

        return BuildWavStream(samples);
    }

    private static MemoryStream BuildWavStream(short[] samples)
    {
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        int subChunk2Size = samples.Length * 2;
        int chunkSize = 36 + subChunk2Size;

        writer.Write(new[] { 'R', 'I', 'F', 'F' });
        writer.Write(chunkSize);
        writer.Write(new[] { 'W', 'A', 'V', 'E' });

        writer.Write(new[] { 'f', 'm', 't', ' ' });
        writer.Write(16);
        writer.Write((short)1); // PCM
        writer.Write((short)1); // Mono
        writer.Write(SampleRate);
        writer.Write(SampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);

        writer.Write(new[] { 'd', 'a', 't', 'a' });
        writer.Write(subChunk2Size);

        for (int i = 0; i < samples.Length; i++)
        {
            writer.Write(samples[i]);
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
