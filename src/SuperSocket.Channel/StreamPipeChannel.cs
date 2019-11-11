using System;
using System.Buffers;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipelines;
using SuperSocket.ProtoBase;

namespace SuperSocket.Channel
{
    public class StreamPipeChannel<TPackageInfo> : PipeChannel<TPackageInfo>
        where TPackageInfo : class
    {
        private Stream _stream;

        public StreamPipeChannel(Stream stream, IPipelineFilter<TPackageInfo> pipelineFilter, ChannelOptions options)
            : base(pipelineFilter, options)
        {
            _stream = stream;
        }

        public override void Close()
        {
            _stream.Close();
        }


        protected override void OnClosed()
        {
            _stream = null;
            base.OnClosed();
        }

        protected override async ValueTask<int> FillPipeWithDataAsync(Memory<byte> memory)
        {
            return await _stream.ReadAsync(memory);
        }

        protected override async ValueTask<int> SendOverIOAsync(ReadOnlySequence<byte> buffer)
        {
            var total = 0;

            foreach (var data in buffer)
            {
                await _stream.WriteAsync(data);
                total += data.Length;
            }

            await _stream.FlushAsync();
            return total;
        }
    }
}