using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public static class WavUtility
{
    /// <summary>
    /// Default header size for uncompressed wav. 
    /// </summary>
    const int HEADER_SIZE = 44;

    /// <summary>
    /// Create file stream and initialize header with placeholders. 
    /// </summary>
    private static FileStream CreateFileStream(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);

        byte[] emptyHeader = new byte[HEADER_SIZE];
        fileStream.Write(emptyHeader, 0, HEADER_SIZE);

        return fileStream;
    }

    /// <summary>
    /// Save the audio to a wav file. 
    /// </summary>
    /// <param name="filePath">The file path to where the audio will be saved.</param>
    /// <param name="clip">The audio clip which will be saved.</param>
    public static void Save(string filePath, AudioClip clip)
    {
        if (!filePath.ToLower().EndsWith(".wav"))
        {
            filePath += ".wav";
        }

        using (FileStream fileStream = CreateFileStream(filePath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }
    }

    /// <summary>
    /// Convert samples to wav format and write to file.
    /// </summary>
    /// <param name="fileStream">The file to which the samples will be written.</param>
    /// <param name="clip">The audio clip from which samples will be extracted.</param>
    private static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        byte[] samplesWav = ConvertToWav(samples);

        fileStream.Write(samplesWav, 0, samplesWav.Length);
    }

    /// <summary>
    /// Convert samples from the format of float to 16-bit, and then to byte. 
    /// </summary>
    /// <param name="samples">A float array of audio data.</param>
    /// <returns>Returns the converted bytes array.</returns>
    private static byte[] ConvertToWav(float[] samples)
    {
        Int16[] intData = new Int16[samples.Length];
        byte[] bytesData = new Byte[samples.Length * 2]; // 16-bit audio

        int rescaleFactor = 32767; // to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (Int16)(samples[i] * rescaleFactor);
            byte[] bytes = BitConverter.GetBytes(intData[i]);
            bytes.CopyTo(bytesData, i * 2);
        }

        return bytesData;
    }

    /// <summary>
    /// Write header of the wav file. 
    /// </summary>
    /// <param name="fileStream">The file to which the header will be written.</param>
    /// <param name="clip">The audio clip from which metadata will be extracted.</param>
    private static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 one = 1;
        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
}