using System.Net.Sockets;
using System.Text;

namespace WebSocketServer.Helper
{
    public static class WebSocketFrame
    {
        public static async Task<string> ReadFrameAsync(NetworkStream stream)
        {
            StringBuilder messageBuilder = new StringBuilder();
            bool isFinalFrame = false;

            while (!isFinalFrame)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead <= 0)
                    return null;

                isFinalFrame = (buffer[0] & 0b10000000) != 0;
                bool isMasked = (buffer[1] & 0b10000000) != 0;
                int payloadLength = buffer[1] & 0b01111111;

                int offset = 2;
                if (payloadLength == 126)
                {
                    payloadLength = BitConverter.ToUInt16(new byte[] { buffer[3], buffer[2] }, 0);
                    offset = 4;
                }
                else if (payloadLength == 127)
                {
                    payloadLength = (int)BitConverter.ToUInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                    offset = 10;
                }

                byte[] maskingKey = new byte[4];
                if (isMasked)
                {
                    Array.Copy(buffer, offset, maskingKey, 0, 4);
                    offset += 4;
                }

                byte[] payloadData = new byte[payloadLength];
                int remainingBytes = payloadLength;
                int dataOffset = 0;

                while (remainingBytes > 0)
                {
                    int toRead = Math.Min(remainingBytes, buffer.Length - offset);
                    Array.Copy(buffer, offset, payloadData, dataOffset, toRead);

                    remainingBytes -= toRead;
                    dataOffset += toRead;
                    offset = 0;

                    if (remainingBytes > 0)
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead <= 0)
                            break;
                    }
                }

                if (isMasked)
                {
                    for (int i = 0; i < payloadData.Length; i++)
                    {
                        payloadData[i] ^= maskingKey[i % 4];
                    }
                }

                messageBuilder.Append(Encoding.UTF8.GetString(payloadData));
            }

            return messageBuilder.ToString();
        }

        public static async Task WriteFrameAsync(NetworkStream stream, string message)
        {
            byte[] payloadData = Encoding.UTF8.GetBytes(message);
            byte[] frame = new byte[2 + payloadData.Length];
            frame[0] = 0b10000001; // FIN bit set and text frame
            frame[1] = (byte)payloadData.Length;
            Array.Copy(payloadData, 0, frame, 2, payloadData.Length);

            await stream.WriteAsync(frame, 0, frame.Length);
        }
    }
}
