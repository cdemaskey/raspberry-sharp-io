#region References

using System;

#endregion

namespace Raspberry.IO.SerialPeripheralInterface
{
    /// <summary>
    /// Represents a SPI slave selection context.
    /// </summary>
    public class SpiSlaveSelectionContext : IDisposable
    {
        #region Fields

        private readonly SpiConnection connection;

        private readonly IOutputBinaryPin selectSlavePin;

        #endregion

        #region Instance Management

        internal SpiSlaveSelectionContext(SpiConnection connection, IOutputBinaryPin selectSlavePin)
        {
            this.connection = connection;
            this.selectSlavePin = selectSlavePin;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            connection.DeselectSlave(this.selectSlavePin);
        }

        #endregion
    }
}