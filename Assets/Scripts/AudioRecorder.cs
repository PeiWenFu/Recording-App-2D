using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    private string _microphone;
    private AudioClip _audioClip;
    private AudioClip _recordedClip;
    private string _fileName;
    private string _filePath;
    private bool _isRecording = false;
    private float _startTime;

    /// <summary>
    /// The duration of the recording in seconds.
    /// </summary>
    private int _lengthSec = 3599;

    /// <summary>
    /// The number of samples taken per second. 
    /// </summary>
    private int _sampleRate;

    /// <summary>
    /// Start recording if microphone is enabled. 
    /// </summary>
    private void Start()
    {
        if (MicrophoneEnabled())
        {
            InitializeMicrophone();
            StartRecording();
        }
        else
        {
            enabled = false;
            DebugLog.Error("Microphone is not enabled. ");
        }
    }

    /// <summary>
    /// Restart recording if the length of the recording has been reached or due to other reasons. 
    /// </summary>
    private void Update()
    {
        if (!_isRecording)
        {
            StartRecording();
        }
        else if (_isRecording && !Microphone.IsRecording(_microphone))
        {
            StopRecording();
            StartRecording();
        }
    }

    /// <summary>
    /// Stop Recording when pause and start recording when resume. 
    /// </summary>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            StopRecording();
        }
        else
        {
            StartRecording();
        }
    }

    /// <summary>
    /// Check if microphone is enabled. 
    /// </summary>
    /// <returns>Returns true if microphone is enabled, false otherwise.</returns>
    private bool MicrophoneEnabled()
    {
        return Microphone.devices.Length >= 1;
    }

    /// <summary>
    /// Get the general-purpose microphone and its maximum sample rate. 
    /// </summary>
    private void InitializeMicrophone()
    {
        // There are three microphone options. Microphone.devices[1] is set as default since it's the clearest. 
        // [0]: Android audio input
        // [1]: Android camcorder input
        // [2]: Android voice recognition
        _microphone = Microphone.devices[1];

        Microphone.GetDeviceCaps(_microphone, out int minFreq, out int maxFreq);
        _sampleRate = maxFreq;
    }

    /// <summary>
    /// Update file path with current time. 
    /// </summary>
    private void UpdateFilePath()
    {
        string basePath = Application.persistentDataPath;
        string timestamp = System.DateTime.Now.ToString("yyyyMMddHHmmss_");
        _fileName = timestamp + _microphone;
        _filePath = Path.Combine(basePath, _fileName + ".wav");
    }

    /// <summary>
    /// Set file name with current time and start recording. 
    /// </summary>
    private void StartRecording()
    {
        UpdateFilePath();

        _audioClip = Microphone.Start(_microphone, false, _lengthSec, _sampleRate);
        _startTime = Time.realtimeSinceStartup;

        if (_audioClip == null)
        {
            DebugLog.Info($"{_microphone} fails to start. \n\n");
        }
        else
        {
            _isRecording = true;
            DebugLog.Info($"{_microphone} starts recording. \n\n");
        }
        
    }

    /// <summary>
    /// Stop Recording and save the audio. 
    /// </summary>
    private void StopRecording()
    {
        if (_isRecording)
        {
            Microphone.End(_microphone);
            float stopTime = Time.realtimeSinceStartup;
            _isRecording = false;
            DebugLog.Info($"{_microphone} stops recording. ");
            
            float recordingLength = stopTime - _startTime;
            if (recordingLength > 0)
            {
                _recordedClip = TrimClip(_audioClip, recordingLength);
                _audioClip = null;

                SaveRecordingToWav();
            }
        }
    }

    /// <summary>
    /// Trim the audio clip to recording length. 
    /// </summary>
    /// <param name="clip">The full audio clip from microphone.</param>
    /// <param name="length">The recording length.</param>
    /// <returns>The trimmed audio clip.</returns>
    private AudioClip TrimClip(AudioClip fullClip, float length)
    {
        if (length > _lengthSec)
        {
            length = _lengthSec;
        }

        int samples = (int)(fullClip.frequency * length);
        float[] data = new float[samples];
        fullClip.GetData(data, 0);

        AudioClip trimmedClip = AudioClip.Create(_fileName, samples, fullClip.channels, fullClip.frequency, false);
        trimmedClip.SetData(data, 0);

        return trimmedClip;
    }

    /// <summary>
    /// Save the recording to a wav file. 
    /// </summary>
    private void SaveRecordingToWav()
    {
        if (_recordedClip != null)
        {
            WavUtility.Save(_filePath, _recordedClip);
        }
        else
        {
            DebugLog.Error("No recording found to save.");
        }
    }
}
