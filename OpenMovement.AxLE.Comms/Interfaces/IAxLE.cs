﻿using System;
using System.IO;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Values;
using OpenMovement.AxLE.Service.Models;

namespace OpenMovement.AxLE.Comms.Interfaces
{
    public interface IAxLE : IDisposable
    {
        /// <summary>
        /// Gets the AxLE's Bluetooth device identifier (specific to install).
        /// </summary>
        /// <value>The device identifier.</value>
        string DeviceId { get; }
        /// <summary>
        /// Gets the AxLE's serial number Format=[XXXXXXXXXXXX].
        /// </summary>
        /// <value>The serial number.</value>
        string SerialNumber { get; }
        /// <summary>
        /// Gets the battery percentage.
        /// </summary>
        /// <value>The battery percentage.</value>
        int Battery { get; }
        /// <summary>
        /// AxLE local device time in seconds.
        /// </summary>
        /// <value>The device time.</value>
        UInt32 DeviceTime { get; }
        /// <summary>
        /// AxLE devices' historic erase data.
        /// </summary>
        /// <value>The erase data.</value>
        EraseData EraseData { get; }
        /// <summary>
        /// BLE device connection interval. Fires off command to device.
        /// </summary>
        UInt32 ConnectionInterval { get; set; }
        /// <summary>
        /// Turn cueuing on or off for 60 seconds. Fires off command to device.
        /// </summary>
        bool Cueing { get; set; }
        /// <summary>
        /// Get or Set cueing period in seconds. Fires off command to device.
        /// </summary>
        UInt32 CueingPeriod { get; set; }
        /// <summary>
        /// Get or Set Epoch period in seconds. Fires off command to device.
        /// </summary>
        UInt32 EpochPeriod { get; set; }
        /// <summary>
        /// Get or Set the goal period offset in seconds. Fires off command to device.
        /// </summary>
        UInt32 GoalPeriodOffset { get; set; }
        /// <summary>
        /// Get or Set the goal period in seconds. Fires off command to device.
        /// </summary>
        UInt32 GoalPeriod { get; set; }
        /// <summary>
        /// Get or Set the step goal threshold (vibrates when exceeded in 24hr period). Fires off command to device.
        /// </summary>
        UInt32 GoalThreshold { get; set; }

        /// <summary>
        /// Subscribe to this callback when running accelerometer stream, called when new stream block is recieved.
        /// </summary>
        event EventHandler<AccBlock> AccelerometerStream;

        /// <summary>
        /// Authenticate with AxLE device using specified password (default password is last 6 digits of serial number).
        /// </summary>
        /// <returns>Success or failure of auth.</returns>
        /// <param name="password">Password.</param>
        Task<bool> Authenticate(string password);
        /// <summary>
        /// Queries various properties of the device and updates AxLE object. Not run on construction for performance reasons.
        /// </summary>
        Task UpdateDeviceState();
        /// <summary>
        /// Sets the password.
        /// </summary>
        /// <param name="password">Password.</param>
        Task SetPassword(string password);
        /// <summary>
        /// Resets the password to last six digits of MAC address (THIS ERASES ALL DEVICE DATA).
        /// </summary>
        Task ResetPassword();
        /// <summary>
        /// Vibrates the device.
        /// </summary>
        Task VibrateDevice();
        /// <summary>
        /// Starts flash pattern on device.
        /// </summary>
        Task LEDFlash();
        /// <summary>
        /// Ask user to confirm interaction. Waits for user to tap screen, times out if user does not respond.
        /// </summary>
        /// <param name="timeout">Time in milliseconds till interaction confirmation timesout.</param>
        /// <returns>If user confirmed interaction.</returns>
        Task<bool> ConfirmUserInteraction(int timeout);
        /// <summary>
        /// Writes a bitmap from a local file to the device at position 0. This operation takes a 32x32 monochrome bitmap only.
        /// </summary>
        /// <exception cref="FileNotFoundException">Thrown when bitmap file could not be opened.</exception>
        /// <param name="file">Path to 32x32 monochrome bitmap.</param>
        Task WriteBitmap(string file);
        /// <summary>
        /// Writes a bitmap from data to the device.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if the data length or offset are not multiples of 8.</exception>
        /// <param name="data">Binary data (typically a 32x32 monochrome bitmap)</param>
        /// <param name="offset">Offset at which to write the data (default is 0)</param>
        Task WriteBitmap(byte[] data, int offset = 0);
        /// <summary>
        /// Clears the display of painted artifacts. This will in time return to the clock face.
        /// </summary>
        Task ClearDisplay();
        /// <summary>
        /// Displays icon on display screen from bitmap <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">Byte offset in memory to display from.</param>
        /// <param name="start">Start row on screen to start display.</param>
        /// <param name="height">Number of rows on screen.</param>
        Task DisplayIcon(int offset, int start, int height);
        /// <summary>
        /// Paints the display with data from loaded bitmap.
        /// </summary>
        /// <param name="offset">Byte offset in bitmap to start paint from.</param>
        /// <param name="startCol">Start column in bitmap.</param>
        /// <param name="startRow">Start row in bitmap.</param>
        /// <param name="cols">Number of columns to paint in display.</param>
        /// <param name="rows">Number of rows to paint in display.</param>
        /// <param name="span">Span to paint in bitmap.</param>
        /// <returns></returns>
        Task PaintDisplay(UInt16 offset, byte startCol, byte startRow, byte cols, byte rows, byte span);
        /// <summary>
        /// Starts streaming accelerometer data. Subscribe to <see cref="AccelerometerStream"/> event to get data.
        /// </summary>
        Task StartAccelerometerStream(int rate = 0, int range = 0);
        /// <summary>
        /// Stops streaming accelerometer data.
        /// </summary>
        Task StopAccelerometerStream();
        /// <summary>
        /// Syncs the Epoch data from the device, operation may take some time. This overload reads to the active block.
        /// </summary>
        /// <exception cref="InvalidBlockRangeException">Thrown when a the requested range exceeds the device storage. You must use the other overload to read.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="lastRtc"/> does not match the band data.</exception>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check twice.</exception>
        /// <returns>The synced Epoch data.</returns>
        /// <param name="lastBlock">Last block read from device, if first run read ActiveBlock from <see cref="ReadBlockDetails"/>.</param>
        /// <param name="lastRtc">Device clock at last sync.</param>
        /// <param name="lastSync">Global time at last sync.</param>
        Task<EpochBlock[]> SyncEpochData(UInt16 lastBlock, UInt32? lastRtc = null, DateTimeOffset? lastSync = null);
        /// <summary>
        /// Syncs the Epoch data from the device, operation may take some time.
        /// </summary>
        /// <exception cref="InvalidBlockRangeException">Thrown when a the requested range exceeds the device storage.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="startRtc"/> does not match the band data.</exception>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check twice.</exception>
        /// <returns>The synced Epoch data.</returns>
        /// <param name="readFrom">Block to read from, if first run read ActiveBlock from <see cref="ReadBlockDetails"/>.</param>
        /// <param name="readTo">Block number to read to, usually ActiveBlock.</param>
        /// <param name="startRtc">Device clock at beginning of block read range.</param>
        /// <param name="startTime">Global time at beginning of block read range.</param>
        Task<EpochBlock[]> SyncEpochData(UInt16 readFrom, UInt16 readTo, UInt32? startRtc = null, DateTimeOffset? startTime = null);
        /// <summary>
        /// Syncs the Epoch block referenced by the DownloadBlock pointer from <see cref="ReadBlockDetails"/>.
        /// </summary>
        /// <exception cref="BlockSyncFailedException">Thrown when a block in the sync operation fails CRC check.</exception>
        /// <returns>The current Epoch block.</returns>
        Task<EpochBlock> SyncCurrentEpochBlock();
        /// <summary>
        /// Writes the current block to be read.
        /// </summary>
        /// <returns>The block details.</returns>
        /// <param name="blockNo">Block no.</param>
        Task<BlockDetails> WriteCurrentBlock(UInt16 blockNo);
        /// <summary>
        /// Reads the block details from the device, run this operation on setup get starting block.
        /// </summary>
        /// <returns>The block details.</returns>
        Task<BlockDetails> ReadBlockDetails();
        /// <summary>
        /// Writes the real-world date/time to the device.
        /// <param name="time">Real-world date/time</param>
        /// </summary>
        Task WriteRealTime(DateTime time);

        /// <summary>
        /// Retrieve Debug output from device.
        /// </summary>
        /// <returns>The dump.</returns>
		Task<string> DebugDump();
    }
}
