﻿using System;
using System.Collections.Generic;
using System.Timers;
using OpenMovement.AxLE.Comms.Interfaces;
using OpenMovement.AxLE.Comms.Exceptions;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Commands;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEProcessor : IAxLEProcessor
    {
        private const double ProcessInterval = 50;
        private const double CommandTimeout = 5000;

        private readonly IAxLEDevice _device;
        private readonly Queue<IAxLECommand> _commandList;

        private Timer Timer { get; set; }
        private bool Processing { get; set; }

        public AxLEProcessor(IAxLEDevice device)
        {
            _device = device;

            Processing = false;
            _commandList = new Queue<IAxLECommand>();
        }

        public void StartProcessor()
        {
            Timer = new Timer
            {
                Interval = ProcessInterval,
                AutoReset = true
            };

            Timer.Elapsed += (t, e) =>
            {
                if (!Processing)
                {
                    ProcessCommandList();
                }
            };
            Timer.Start();
        }

        public void StopProcessor()
        {
            Timer.Stop();
            Timer.Dispose();
        }

        private async void ProcessCommandList()
        {
            if (!Processing)
            {
                while (_commandList.Count > 0)
                {
                    Processing = true;
                    var command = _commandList.Dequeue();
                    command.Device = _device;
                    try
                    {
                        await command.Execute();
                    }
                    catch (CommandFailedException e)
                    {
                        Console.WriteLine($"COMMAND FAILED: {e}");
                    }
                }
                Processing = false;
            }
        }

        public Task<T> AddCommand<T>(AxLECommand<T> command)
        {
            _commandList.Enqueue(command);
            return command.TCS.Task;
        }

        public Task AddCommand<T>(AxLECommandStream<T> command)
        {
            _commandList.Enqueue(command);
            return command.TCS.Task;
        }

        public void Dispose()
        {
            StopProcessor();
        }
    }
}