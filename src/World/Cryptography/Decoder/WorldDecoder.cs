﻿using System;
using System.Collections.Generic;
using System.Text;
using ChickenAPI.Core.i18n;
using ChickenAPI.Game._Network;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Common.Utilities;
using DotNetty.Transport.Channels;
using World.Network;
using World.Utils;

namespace World.Cryptography.Decoder
{
    public class WorldDecoder : MessageToMessageDecoder<IByteBuffer>, IDecoder
    {
        private Encoding _encoding = Encoding.Default;
        private ISession _session;
        private LanguageKey _language = LanguageKey.EN;
        private int _sessionId = -1;

        private string DecryptPrivate(string str)
        {
            List<byte> receiveData = new List<byte>();
            char[] table = { ' ', '-', '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'n' };
            for (int count = 0; count < str.Length; count++)
            {
                if (str[count] <= 0x7A)
                {
                    int len = str[count];

                    for (int i = 0; i < len; i++)
                    {
                        count++;

                        try
                        {
                            receiveData.Add(unchecked((byte)(str[count] ^ 0xFF)));
                        }
                        catch
                        {
                            receiveData.Add(255);
                        }
                    }
                }
                else
                {
                    int len = str[count];
                    len &= 0x7F;

                    for (int i = 0; i < len; i++)
                    {
                        count++;
                        int highbyte;
                        try
                        {
                            highbyte = str[count];
                        }
                        catch
                        {
                            highbyte = 0;
                        }

                        highbyte &= 0xF0;
                        highbyte >>= 0x4;

                        int lowbyte;
                        try
                        {
                            lowbyte = str[count];
                        }
                        catch
                        {
                            lowbyte = 0;
                        }

                        lowbyte &= 0x0F;

                        if (highbyte != 0x0 && highbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte)table[highbyte - 1]));
                            i++;
                        }

                        if (lowbyte != 0x0 && lowbyte != 0xF)
                        {
                            receiveData.Add(unchecked((byte)table[lowbyte - 1]));
                        }
                    }
                }
            }

            return _encoding.GetString(receiveData.ToArray());
        }

        public string DecryptCustomParameter(Span<byte> str)
        {
            try
            {
                var encryptedStringBuilder = new StringBuilder();
                for (int i = 1; i < str.Length; i++)
                {
                    if (Convert.ToChar(str[i]) == 0xE)
                    {
                        return encryptedStringBuilder.ToString();
                    }

                    int firstbyte = Convert.ToInt32(str[i] - 0xF);
                    int secondbyte = firstbyte;
                    secondbyte &= 240;
                    firstbyte = Convert.ToInt32(firstbyte - secondbyte);
                    secondbyte >>= 4;

                    switch (secondbyte)
                    {
                        case 0:
                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            secondbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(secondbyte));
                            break;
                    }

                    switch (firstbyte)
                    {
                        case 0:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 1:
                            encryptedStringBuilder.Append(' ');
                            break;

                        case 2:
                            encryptedStringBuilder.Append('-');
                            break;

                        case 3:
                            encryptedStringBuilder.Append('.');
                            break;

                        default:
                            firstbyte += 0x2C;
                            encryptedStringBuilder.Append(Convert.ToChar(firstbyte));
                            break;
                    }
                }

                return encryptedStringBuilder.ToString();
            }
            catch (OverflowException)
            {
                return string.Empty;
            }
        }

        private string Decode(Span<byte> str)
        {
            var encryptedString = new StringBuilder();

            int sessionKey = _sessionId & 0xFF;
            byte sessionNumber = unchecked((byte)(_sessionId >> 6));
            sessionNumber &= 0xFF;
            sessionNumber &= 3;

            switch (sessionNumber)
            {
                case 0:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(sessionKey + 0x40));
                        byte highbyte = unchecked((byte)(character - firstbyte));
                        encryptedString.Append((char)highbyte);
                    }

                    break;

                case 1:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(sessionKey + 0x40));
                        byte highbyte = unchecked((byte)(character + firstbyte));
                        encryptedString.Append((char)highbyte);
                    }

                    break;

                case 2:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(sessionKey + 0x40));
                        byte highbyte = unchecked((byte)(character - firstbyte ^ 0xC3));
                        encryptedString.Append((char)highbyte);
                    }

                    break;

                case 3:
                    foreach (byte character in str)
                    {
                        byte firstbyte = unchecked((byte)(sessionKey + 0x40));
                        byte highbyte = unchecked((byte)(character + firstbyte ^ 0xC3));
                        encryptedString.Append((char)highbyte);
                    }

                    break;

                default:
                    encryptedString.Append((char)0xF);
                    break;
            }

            Span<string> temp = encryptedString.ToString().Split((char)0xFF);

            var save = new StringBuilder();

            for (int i = 0; i < temp.Length; i++)
            {
                save.Append(DecryptPrivate(temp[i]));
                if (i < temp.Length - 2)
                {
                    save.Append((char)0xFF);
                }
            }

            return save.ToString();
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            if (!message.IsReadable())
            {
                return;
            }

            Span<byte> str = message.Array.Slice(message.ArrayOffset, message.ReadableBytes);

            // _sessionId will only change
            if (_sessionId == -1)
            {
                if (!SocketSessionManager.Instance.GetSession(context.Channel.Id.AsLongText(), out ISession psession))
                {
                    output.Add(DecryptCustomParameter(str));
                    return;
                }

                _session = psession;
                _sessionId = _session.SessionId;
                _language = _session.Language;
            }

            // made for runtime language changes
            if (_language != _session.Language)
            {
                _encoding = _language.GetEncoding();
            }

            output.Add(Decode(str));
        }
    }
}