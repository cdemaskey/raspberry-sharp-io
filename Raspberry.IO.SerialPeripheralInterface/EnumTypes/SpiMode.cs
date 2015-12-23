using System;

namespace Raspberry.IO.SerialPeripheralInterface
{
    /// <summary>
    /// SPI mode
    /// </summary>
    [Flags]
    public enum SpiMode : uint
    {
        /// <summary>
        /// Clock phase, if set CPHA=1, otherwise CPHA=0.
        /// </summary>
        ClockPhase = Interop.SPI_CPHA,

        /// <summary>
        /// Clock polarity, if set CPOL=1, otherwise CPOL=0.
        /// </summary>
        ClockPolarity = Interop.SPI_CPOL,
        
        /// <summary>
        /// Chip select is a high signal.
        /// </summary>
        ChipSelectActiveHigh = Interop.SPI_CS_HIGH,

        /// <summary>
        /// Send no chip select signal.
        /// </summary>
        NoChipSelect = Interop.SPI_NO_CS,
    }
}