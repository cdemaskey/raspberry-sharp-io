#region References

using System;
using Raspberry.Timers;

#endregion

namespace Raspberry.IO.SerialPeripheralInterface
{
    /// <summary>
    /// Represents a connection to a SPI device.
    /// </summary>
    public class SpiConnection : IDisposable
    {
        #region Fields

        private readonly IOutputBinaryPin clockPin;
        private readonly IOutputBinaryPin selectSlave1Pin;
        private readonly IOutputBinaryPin selectSlave2Pin;
        private readonly IInputBinaryPin misoPin;
        private readonly IOutputBinaryPin mosiPin;

        private readonly Endianness endianness = Endianness.LittleEndian;

        private static readonly TimeSpan syncDelay = TimeSpan.FromMilliseconds(1);

        #endregion

        #region Instance Management

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiConnection"/> class.
        /// </summary>
        /// <param name="clockPin">The clock pin.</param>
        /// <param name="selectSlavePin">The select slave pin.</param>
        /// <param name="misoPin">The miso pin.</param>
        /// <param name="mosiPin">The mosi pin.</param>
        /// <param name="endianness">The endianness.</param>
        public SpiConnection(IOutputBinaryPin clockPin, IOutputBinaryPin selectSlavePin, IInputBinaryPin misoPin, IOutputBinaryPin mosiPin, Endianness endianness):
            this(clockPin, selectSlavePin, null, misoPin, mosiPin, endianness)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpiConnection"/> class.
        /// </summary>
        /// <param name="clockPin">The clock pin.</param>
        /// <param name="selectSlave1Pin">The select slave 1 pin.</param>
        /// <param name="selectSlave2Pin">The select slave 2 pin.</param>
        /// <param name="misoPin">The miso pin.</param>
        /// <param name="mosiPin">The mosi pin.</param>
        /// <param name="endianness">The endianness.</param>
        public SpiConnection(IOutputBinaryPin clockPin, IOutputBinaryPin selectSlave1Pin, IOutputBinaryPin selectSlave2Pin, IInputBinaryPin misoPin, IOutputBinaryPin mosiPin, Endianness endianness)
        {
            this.clockPin = clockPin;
            this.selectSlave1Pin = selectSlave1Pin;
            this.selectSlave2Pin = selectSlave2Pin;
            this.misoPin = misoPin;
            this.mosiPin = mosiPin;
            this.endianness = endianness;

            this.clockPin.Write(false);
            this.selectSlave1Pin.Write(true);

            if(this.selectSlave2Pin != null)
            {
                this.selectSlave2Pin.Write(false);
            }

            if (mosiPin != null)
            {
                mosiPin.Write(false);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            Close();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            clockPin.Dispose();
            selectSlave1Pin.Dispose();
            if (selectSlave2Pin != null)
            {
                selectSlave2Pin.Dispose();
            }
            if (mosiPin != null)
            {
                mosiPin.Dispose();
            }
            if (misoPin != null)
            {
                misoPin.Dispose();
            }
        }

        /// <summary>
        /// Selects the slave 1 device.
        /// </summary>
        /// <returns>The slave selection context.</returns>
        public SpiSlaveSelectionContext SelectSlave1()
        {
            selectSlave1Pin.Write(false);
            return new SpiSlaveSelectionContext(this, selectSlave1Pin);
        }

        /// <summary>
        /// Selects the slave 2 device.
        /// </summary>
        /// <returns>The slave selection context.</returns>
        public SpiSlaveSelectionContext SelectSlave2()
        {
            selectSlave2Pin.Write(false);
            return new SpiSlaveSelectionContext(this, selectSlave2Pin);
        }

        /// <summary>
        /// Synchronizes the devices.
        /// </summary>
        public void Synchronize()
        {
            clockPin.Write(true);
            Timer.Sleep(syncDelay);
            clockPin.Write(false);
        }

        /// <summary>
        /// Writes the specified bit to the device.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Write(bool data)
        {
            if (mosiPin == null)
                throw new NotSupportedException("No MOSI pin has been provided");

            mosiPin.Write(data);
            Synchronize();
        }

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bitCount">The bit count.</param>
        public void Write(byte data, int bitCount)
        {
            if (bitCount > 8)
                throw new ArgumentOutOfRangeException("bitCount", bitCount, "byte data cannot contain more than 8 bits");

            SafeWrite(data, bitCount);
        }

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bitCount">The bit count.</param>
        public void Write(ushort data, int bitCount)
        {
            if (bitCount > 16)
                throw new ArgumentOutOfRangeException("bitCount", bitCount, "ushort data cannot contain more than 16 bits");

            SafeWrite(data, bitCount);
        }

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bitCount">The bit count.</param>
        public void Write(uint data, int bitCount)
        {
            if (bitCount > 32)
                throw new ArgumentOutOfRangeException("bitCount", bitCount, "uint data cannot contain more than 32 bits");

            SafeWrite(data, bitCount);
        }

        /// <summary>
        /// Writes the specified data.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="bitCount">The bit count.</param>
        public void Write(ulong data, int bitCount)
        {
            if (bitCount > 64)
                throw new ArgumentOutOfRangeException("bitCount", bitCount, "ulong data cannot contain more than 64 bits");

            SafeWrite(data, bitCount);
        }

        /// <summary>
        /// Reads a bit from the device.
        /// </summary>
        /// <returns>The bit status.</returns>
        public bool Read()
        {
            if (misoPin == null)
                throw new NotSupportedException("No MISO pin has been provided");

            Synchronize();
            return misoPin.Read();
        }

        /// <summary>
        /// Reads the specified number of bits from the device.
        /// </summary>
        /// <param name="bitCount">The bit count.</param>
        /// <returns>The read value.</returns>
        public ulong Read(int bitCount)
        {
            if (bitCount > 64)
                throw new ArgumentOutOfRangeException("bitCount", bitCount, "ulong data cannot contain more than 64 bits");

            ulong data = 0;
            for (var i = 0; i < bitCount; i++)
            {
                var index = endianness == Endianness.BigEndian
                                ? i
                                : bitCount - 1 - i;

                var bit = Read();
                if (bit)
                    data |= ((ulong)1 << index);
            }

            return data;
        }

        #endregion

        #region Internal Methods

        internal void DeselectSlave(IOutputBinaryPin selectSlavePin)
        {
            selectSlavePin.Write(true);
        }

        #endregion

        #region Private Helpers

        private void SafeWrite(ulong data, int bitCount)
        {
            for (var i = 0; i < bitCount; i++)
            {
                var index = endianness == Endianness.BigEndian
                                ? i
                                : bitCount - 1 - i;

                var bit = data & ((ulong)1 << index);
                Write(bit != 0);
            }
        }

        #endregion
    }
}