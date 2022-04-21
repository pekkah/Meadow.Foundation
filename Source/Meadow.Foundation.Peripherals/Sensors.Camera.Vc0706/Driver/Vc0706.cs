﻿using Meadow.Hardware;
using System;
using System.Text;
using System.Threading;

namespace Meadow.Foundation.Sensors.Camera
{
    /// <summary>
    /// Class that represents a VC0706 serial VGA camera
    /// </summary>
    public partial class Vc0706 : ICamera
    {
        readonly ISerialPort serialPort;
        readonly byte serialNumber;
        readonly byte[] cameraBuffer = new byte[CAMERABUFFSIZE + 1];

        byte bufferLength;
        ushort framePointer;

        /// <summary>
        /// Number of bytes avaliable in the camera buffer
        /// </summary>
        public byte BytesAvailable => bufferLength;

        /// <summary>
        /// Create a new VC0706 serial camera object
        /// </summary>
        /// <param name="device"></param>
        /// <param name="portName"></param>
        /// <param name="baudRate"></param>
        public Vc0706(ISerialController device, SerialPortName portName, int baudRate)
        {
             serialPort = device.CreateSerialPort(portName, baudRate);
            /*serialPort = device.CreateSerialMessagePort(
                        portName: portName, 
                        suffixDelimiter: Encoding.ASCII.GetBytes("\r\n"),
                        baudRate: baud,
                        preserveDelimiter: true, 
                        readBufferSize: 512);*/
            serialPort.Open();

            SetBaud((BaudRate)baudRate);
        }

        bool Reset()
        {
            byte[] args = { 0x0 };

            return RunCommand(RESET, args, 1, 5);
        }

        /// <summary>
        /// Check if camera has detected recent motion
        /// </summary>
        /// <returns></returns>
        public bool IsMotionDetected()
        {
            if (ReadResponse(4) != 4)
            {
                return false;
            }
            if (!VerifyResponse(COMM_MOTION_DETECTED))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the motion detection mode
        /// </summary>
        /// <param name="x"></param>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public bool SetMotionStatus(byte x, byte d1, byte d2)
        {
            byte[] args = { 0x03, x, d1, d2 };

            return RunCommand(MOTION_CTRL, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Get the motion detection mode
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public bool GetMotionStatus(byte x)
        {
            byte[] args = { 0x01, x };

            return RunCommand(MOTION_STATUS, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Enable or disable motion detection
        /// </summary>
        /// <param name="enable">true to enable</param>
        /// <returns>true if succesful</returns>
        public bool SetMotionDetect(bool enable)
        {
            if (!SetMotionStatus(MOTIONCONTROL, UARTMOTION, ACTIVATEMOTION))
                return false;

            byte[] args = { 0x01, (byte)((enable == true)?1:0) };

            return RunCommand(COMM_MOTION_CTRL, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Get the motion detection mode 
        /// </summary>
        /// <returns>true if enabled, false if not enabled or command failed</returns>
        public bool GetMotionDetect()
        {
            byte[] args = { 0x0 };

            if (!RunCommand(COMM_MOTION_STATUS, args, 1, 6))
            {
                return false;
            }

            return cameraBuffer[5] != 0;
        }

        /// <summary>
        /// Get the current image capture resolution 
        /// </summary>
        /// <returns>the image resolution as an ImageResolution enum</returns>
        public ImageResolution GetCaptureResolution()
        {
            byte[] args = { 0x4, 0x4, 0x1, 0x00, 0x19 };
            if (!RunCommand(READ_DATA, args, (byte)args.Length, 6))
            { 
                return ImageResolution.Unknown; 
            }

            return (ImageResolution)cameraBuffer[5];
        }

        /// <summary>
        /// Set the image resolution
        /// </summary>
        /// <param name="resolution">the new image capture resolution</param>
        /// <returns>true if succesful</returns>
        public bool SetCaptureResolution(ImageResolution resolution)
        {
            byte[] args = { 0x05, 0x04, 0x01, 0x00, 0x19, (byte)resolution };

            var ret = RunCommand(WRITE_DATA, args, (byte)args.Length, 5);
            Reset();
            return ret;
        }

        /// <summary>
        /// Get the downsize value
        /// </summary>
        /// <returns></returns>
        public byte GetDownsize()
        {
            byte[] args = { 0x0 };
            if (RunCommand(DOWNSIZE_STATUS, args, 1, 6) == false)
            {
                return 0;
            }

            return cameraBuffer[5];
        }

        /// <summary>
        /// Set downsize
        /// </summary>
        /// <param name="newSize"></param>
        /// <returns></returns>
        public bool SetDownsize(byte newSize)
        {
            byte[] args = { 0x01, newSize };

            return RunCommand(DOWNSIZE_CTRL, args, 2, 5);
        }

        /// <summary>
        /// Get the camera version
        /// </summary>
        /// <returns>the version as a string</returns>
        public string GetVersion()
        {
            SendCommand(GEN_VERSION, new byte[]{ 0x01 }, 1);
            // get reply
            if (ReadResponse(CAMERABUFFSIZE) == 0)
            { return string.Empty; }

            cameraBuffer[bufferLength] = 0; 

            var versionData = new byte[14];
            Array.Copy(cameraBuffer, 5, versionData, 0, 11);
            versionData[12] = versionData[13] = 0;

            return new string(new UTF8Encoding().GetChars(versionData));
        }

        /// <summary>
        /// Set the serial baud rate for the camera
        /// </summary>
        /// <param name="baudRate">the baud rate</param>
        /// <returns>true if succesful</returns>
        bool SetBaud(BaudRate baudRate)
        {
            byte[] args = baudRate switch
            {
                BaudRate._57600 => new byte[] { 0x03, 0x01, 0x1C, 0x1C },
                BaudRate._38400 => new byte[] { 0x03, 0x01, 0x2A, 0xF2 },
                BaudRate._19200 => new byte[] { 0x03, 0x01, 0x56, 0xE4 },
                _ => new byte[] { 0x03, 0x01, 0xAE, 0xC8 },
            };
            SendCommand(SET_PORT, args, (byte)args.Length);

            // get reply
            if (ReadResponse(CAMERABUFFSIZE) == 0)
            {
                return false;
            }

            cameraBuffer[bufferLength] = 0; // end it!
            return true;
        }

        /// <summary>
        /// Enable onscreen display for composite output (may not work)
        /// </summary>
        /// <param name="x">x location of display in pixels</param>
        /// <param name="y">y location of dispaly in pixels</param>
        /// <param name="message">text to display</param>
        public void SetOnScreenDisplay(byte x, byte y, string message)
        {
            if (message.Length > 14)
            {
                message = message[..14];
            }

            var args = new byte[17];
            args[0] = (byte)message.Length;
            args[1] = (byte)(message.Length - 1);
            args[2] = (byte)((y & 0xF) | ((x & 0x3) << 4));

            for (byte i = 0; i < message.Length; i++)
            {
                char c = message[i];

                if ((c >= '0') && (c <= '9'))
                {
                    args[3 + i] = (byte)(c - '0');
                }
                else if ((c >= 'A') && (c <= 'Z'))
                {
                    args[3 + i] = (byte)(c - 'A');
                    args[3 + i] += 10;
                }
                else if ((c >= 'a') && (c <= 'z'))
                {
                    args[3 + i] = (byte)(c - 'a');
                    args[3 + i] += 36;
                }
            }

            RunCommand(OSD_ADD_CHAR, args, (byte)args.Length, 5);
           // printBuff();
        }

        /// <summary>
        /// Set compression (0-255)
        /// </summary>
        /// <param name="compression"></param>
        /// <returns>true if succesful</returns>
        public bool SetCompression(byte compression)
        {
            byte[] args = { 0x5, 0x1, 0x1, 0x12, 0x04, compression };
            return RunCommand(WRITE_DATA, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Get compression (0-255)
        /// </summary>
        /// <returns>compression value</returns>
        public byte GetCompression()
        {
            byte[] args = { 0x4, 0x1, 0x1, 0x12, 0x04 };
            RunCommand(READ_DATA, args, (byte)args.Length, 6);
            PrintBuffer();
            return cameraBuffer[5];
        }

        /// <summary>
        /// Set Pan, tilt and zoom
        /// </summary>
        /// <param name="horizontalZoom"></param>
        /// <param name="verticalZoom"></param>
        /// <param name="pan"></param>
        /// <param name="tilt"></param>
        /// <returns></returns>
        public bool SetPanTiltZoom(ushort horizontalZoom, ushort verticalZoom, ushort pan, ushort tilt)
        {
            byte[] args = {0x08, 
                            (byte)(horizontalZoom >> 8), (byte)horizontalZoom, 
                            (byte)(verticalZoom >> 8), (byte)horizontalZoom,
                            (byte)(pan >> 8), (byte)pan,     
                            (byte)(tilt >> 8), (byte)tilt};

            return (!RunCommand(SET_ZOOM, args, (byte)args.Length, 5));
        }

        /// <summary>
        /// Get Pan, Tilt and Zoom values
        /// </summary>
        /// <returns></returns>
        public (ushort width, ushort height, ushort horizonalZoom, ushort verticalZoom, ushort pan, ushort tilt) GetPanTiltZoom()
        {
            byte[] args = { 0x0 };

            if (!RunCommand(GET_ZOOM, args, (byte)args.Length, 16))
            { return (0,0,0,0,0,0); }

            PrintBuffer();

            ushort w = cameraBuffer[5];
            w <<= 8;
            w |= cameraBuffer[6];

            ushort h = cameraBuffer[7];
            h <<= 8;
            h |= cameraBuffer[8];

            ushort wz = cameraBuffer[9];
            wz <<= 8;
            wz |= cameraBuffer[10];

            ushort hz = cameraBuffer[11];
            hz <<= 8;
            hz |= cameraBuffer[12];

            ushort pan = cameraBuffer[13];
            pan <<= 8;
            pan |= cameraBuffer[14];

            ushort tilt = cameraBuffer[15];
            tilt <<= 8;
            tilt |= cameraBuffer[16];

            return (w, h, wz, hz, pan, tilt);
        }

        /// <summary>
        /// Capture a new image
        /// </summary>
        /// <returns>true if successful</returns>
        public bool TakePicture()
        {
            framePointer = 0;
            return CameraFrameBuffCtrl(STOPCURRENTFRAME);
        }

        /// <summary>
        /// Resume live video over composite 
        /// </summary>
        /// <returns></returns>
        public bool ResumeVideo()
        {
            return CameraFrameBuffCtrl(RESUMEFRAME);
        }

        /// <summary>
        /// Enable TV output over composite
        /// </summary>
        /// <returns></returns>
        public bool TvOn()
        {
            byte[] args = { 0x1, 0x1 };
            return RunCommand(TVOUT_CTRL, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Disable TV output over composite
        /// </summary>
        /// <returns></returns>
        public bool TvOff()
        {
            byte[] args = { 0x1, 0x0 };
            return RunCommand(TVOUT_CTRL, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Get the current camera color mode (Color, Black and White, automatic)
        /// </summary>
        /// <returns></returns>
        public ColorMode GetColorMode()
        {
            RunCommand(COLOR_STATUS, new byte[] { 0x1 }, 1, 8);
            return (ColorMode)cameraBuffer[7];
        }

        /// <summary>
        /// Set the camera color mode (Color, Black and White, automatic)
        /// </summary>
        /// <param name="colorControl"></param>

        public void SetColorMode(ColorMode colorControl)
        {
            RunCommand(COLOR_CTRL, new byte[] { 0x1, (byte)colorControl }, 2, 5);
        }

        bool CameraFrameBuffCtrl(byte command)
        {
            byte[] args = { 0x1, command };
            return RunCommand(FBUF_CTRL, args, (byte)args.Length, 5);
        }

        /// <summary>
        /// Get the length of the current frame
        /// </summary>
        /// <returns></returns>
        public uint GetFrameLength()
        {
            byte[] args = { 0x01, 0x00 };
            if (!RunCommand(GET_FBUF_LEN, args, (byte)args.Length, 9))
                return 0;

            uint len;
            len = cameraBuffer[5];
            len <<= 8;
            len |= cameraBuffer[6];
            len <<= 8;
            len |= cameraBuffer[7];
            len <<= 8;
            len |= cameraBuffer[8];

            return len;
        }

        /// <summary>
        /// Read bytes from the camera buffer
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public byte[] ReadPicture(byte length)
        {
            byte[] args = {0x0C,
                    0x0,
                    0x0A,
                    0,
                    0,
                    (byte)(framePointer >> 8),
                    (byte)(framePointer & 0xFF),
                    0,
                    0,
                    0,
                    length,
                    (byte)(CAMERADELAY >> 8),
                    (byte)(CAMERADELAY & 0xFF)};

            if (!RunCommand(READ_FBUF, args, (byte)args.Length, 5, false))
            { 
                return new byte[0]; 
            }

            // read into the buffer PACKETLEN!
            if (ReadResponse((byte)(length + 5), CAMERADELAY) == 0)
            { 
                return new byte[0]; 
            }

            framePointer += length;

            return cameraBuffer;
        }

        bool RunCommand(byte cmd, 
                        byte[] args, 
                        byte argn,
                        byte resplen, 
                        bool flushflag = true)
        {   // flush out anything in the buffer?
            if (flushflag)
            {
                ReadResponse(100, 10);
            }

            SendCommand(cmd, args, argn);
            if (ReadResponse(resplen) != resplen)
            {
                return false;
            }
            if (!VerifyResponse(cmd))
            {
                return false;
            }
            return true;
        }

        void SendCommand(byte cmd, byte[] args = null, byte argn = 0)
        {
            serialPort.Write(new byte[] { 0x56, serialNumber, cmd });

            for (byte i = 0; i < argn; i++)
            {
                serialPort.Write(new byte[] { args[i] });
            }
        }

        byte ReadResponse(byte numbytes, byte timeout = 200)
        {
            byte counter = 0;
            bufferLength = 0;
            int avail;

            //100 != 0 && 200 != 0
            while ((bufferLength != numbytes) && (timeout != counter))
            {
                avail = serialPort.BytesToRead;

                if (avail <= 0)
                {
                    Thread.Sleep(1);
                    counter++;
                    continue;
                }
                counter = 0;
                // there's a byte!
                cameraBuffer[bufferLength++] = (byte)serialPort.ReadByte(); // hwSerial->read();
                //Console.WriteLine($"{avail}: A byte! {camerabuff[bufferLen - 1]}");
            }
            return bufferLength;
        }

        bool VerifyResponse(byte command)
        {
            if ((cameraBuffer[0] != 0x76) || (cameraBuffer[1] != serialNumber) ||
                (cameraBuffer[2] != command) || (cameraBuffer[3] != 0x0))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write the current buffer to the console for debugging
        /// </summary>
        public void PrintBuffer()
        {
            for (byte i = 0; i < bufferLength; i++)
            {
                Console.WriteLine(cameraBuffer[i]);
            }
        }
    }
}