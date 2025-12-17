// Copyright (c) Meta Platforms, Inc. and affiliates.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Meta.XR.Samples;
using UnityEngine;

namespace com.meta.xr.colocation.pun2.debug
{
    /// <summary>
    ///     A class that saves application level logs to file
    /// </summary>
    [DefaultExecutionOrder(-10000)] // Set the execution order very early to catch as many startup logs as possible
    [MetaCodeSample("LocalMultiplayerMR-PUN2")]
    public class DebugFileLogger : MonoBehaviour
    {
        private static readonly int InitialLogBufferBytes = 4096;

        [SerializeField] private bool logInEditor = false;

        private string _logFilePath;
        private byte[] _logByteBuffer = new byte[InitialLogBufferBytes];

        private FileStream _fileStream;
        private Task _activeLogTask;

        private bool _applicationQuitting = false;
        private bool _streamDirty = false;
        private int _flushTickDelay;

        private void Awake()
        {
            Logger.SetAllLogsVisibility(true);
            string productName = Regex.Replace(Application.productName, @"[^A-Za-z]+", String.Empty);
            string filename = $"{productName}_Logs_{GetDateAndTime()}.txt";
            _logFilePath = Path.Combine(Application.persistentDataPath, filename);
            _flushTickDelay = Mathf.Max(Application.targetFrameRate, 60) / 2;
        }

        private void OnEnable()
        {
            if (Application.isEditor && !logInEditor)
            {
                this.enabled = false;
                return;
            }

            _fileStream = File.Open(_logFilePath, FileMode.OpenOrCreate);
            Application.logMessageReceived += Log;
            Debug.Log(
                $"{nameof(DebugFileLogger)}: Writing logs to {_logFilePath}, max buffer size {_logByteBuffer.Length}.");
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
            Debug.Log($"{nameof(DebugFileLogger)}: Application quitting.");
        }

        private void OnDisable()
        {
            if (_applicationQuitting)
            {
                return;
            }

            Application.logMessageReceived -= Log;
            CloseStream();
        }

        private void Update()
        {
            if (!_streamDirty
                || (_activeLogTask != null && !_activeLogTask.IsCompleted)
                || Time.frameCount % _flushTickDelay != 0) // Only flush every n ticks
            {
                return;
            }

            _activeLogTask = _fileStream.FlushAsync();
        }

        private void Log(string message, string stacktrace, LogType type)
        {
            LogToFile(message);
            if (type == LogType.Exception)
            {
                LogToFile(stacktrace);
            }
        }

        private async void LogToFile(string message)
        {
            string logText = $"{GetTime()} {message}\n";

            if (_activeLogTask != null && !_activeLogTask.IsCompleted)
            {
                // Await the active task since we use a single byte buffer, don't run multiple simultaneously
                await _activeLogTask;
            }

            // Check that the max possible byte size fits in the allocated buffer. If it doesn't, check if the actual byte size fits, and if it doesn't either then reallocate.
            if (Encoding.Default.GetMaxByteCount(logText.Length) > _logByteBuffer.Length)
            {
                int byteCount = Encoding.Default.GetByteCount(logText);
                if (byteCount > _logByteBuffer.Length)
                {
                    Debug.Log($"{nameof(DebugFileLogger)}: Resizing buffer to {byteCount} bytes.");
                    _logByteBuffer = new byte[(int)(byteCount * 1.5f)];
                }
            }

            int numBytes = Encoding.Default.GetBytes(logText, 0, logText.Length, _logByteBuffer, 0);
            _activeLogTask = _fileStream.WriteAsync(_logByteBuffer, 0, numBytes);
            _streamDirty = true;

            // When quitting, write synchronously
            if (_applicationQuitting)
            {
                await _activeLogTask;
                _fileStream.Flush();
            }
        }

        private async void CloseStream()
        {
            if (_activeLogTask != null && _activeLogTask.Status == TaskStatus.Running)
            {
                await _activeLogTask;
            }

            if (_fileStream != null)
            {
                _fileStream.Flush();
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        private static string GetDateAndTime() => DateTime.Now.ToString("yy-MM-dd_HH-mm-ss");
        private static string GetTime() => DateTime.Now.ToString("HH:mm:ss.fff");
    }
}
