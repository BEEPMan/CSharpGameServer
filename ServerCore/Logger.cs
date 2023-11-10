using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Logger
    {
        private FileStream _fileStream;
        private StreamWriter _streamWriter;
        private bool _isRunning;
        object _lock = new object();
        private Queue<string> _messageQueue = new Queue<string>();

        public Logger(string path)
        {
            _fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, true);
            _streamWriter = new StreamWriter(_fileStream);
            _isRunning = true;
        }

        public void Log(string message)
        {
            if(_isRunning)
            {
                lock(_lock)
                {
                    _messageQueue.Enqueue(message);
                }
            }
        }

        public async Task WriteAsync(string message)
        {
            if(_isRunning)
            {
                if (_fileStream.Position + message.Length > _fileStream.Length)
                {
                    await _streamWriter.FlushAsync();
                }
                await _streamWriter.WriteLineAsync(message);
            }
        }

        public async Task CloseAsync()
        {
            if(_isRunning)
            {
                _isRunning = false;
                await _streamWriter.FlushAsync();
                await _streamWriter.DisposeAsync();
                await _fileStream.DisposeAsync();
            }
        }
    }
}
