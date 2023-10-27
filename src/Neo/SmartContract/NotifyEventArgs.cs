// Copyright (C) 2015-2022 The Neo Project.
// 
// The neo is free software distributed under the MIT software license, 
// see the accompanying file LICENSE in the main directory of the
// project or http://www.opensource.org/licenses/mit-license.php 
// for more details.
// 
// Redistribution and use in source and binary forms with or without
// modifications are permitted.

using Neo.Cryptography;
using Neo.IO;
using Neo.Network.P2P.Payloads;
using Neo.VM;
using Neo.VM.Types;
using System;
using System.IO;
using Array = Neo.VM.Types.Array;

namespace Neo.SmartContract
{
    /// <summary>
    /// The <see cref="EventArgs"/> of <see cref="ApplicationEngine.Notify"/>.
    /// </summary>
    public class NotifyEventArgs : EventArgs, IInteroperable
    {
        /// <summary>
        /// The container that containing the executed script.
        /// </summary>
        public IVerifiable ScriptContainer { get; }

        /// <summary>
        /// The script hash of the contract that sends the log.
        /// </summary>
        public UInt160 ScriptHash { get; }

        /// <summary>
        /// The name of the event.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// The arguments of the event.
        /// </summary>
        public Array State { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyEventArgs"/> class.
        /// </summary>
        /// <param name="container">The container that containing the executed script.</param>
        /// <param name="script_hash">The script hash of the contract that sends the log.</param>
        /// <param name="eventName">The name of the event.</param>
        /// <param name="state">The arguments of the event.</param>
        public NotifyEventArgs(IVerifiable container, UInt160 script_hash, string eventName, Array state)
        {
            this.ScriptContainer = container;
            this.ScriptHash = script_hash;
            this.EventName = eventName;
            this.State = state;
        }

        /// <summary>
        /// Get notification Hash, or UInt256.Zero if something is wrong
        /// </summary>
        public UInt256 GetNotificationHash()
        {
            using MemoryStream ms = new();
            using BinaryWriter writer = new(ms, Utility.StrictUTF8, true);
            writer.Write(ScriptHash);

            try
            {
                writer.Write(EventName);
                writer.Write(BinarySerializer.Serialize(State, 32));
            }
            catch
            {
                // It might have more state entries than expected or an unsupported event name encoding.

                return UInt256.Zero;
            }

            writer.Flush();
            return new UInt256(ms.ToArray().Sha256());
        }

        public void FromStackItem(StackItem stackItem)
        {
            throw new NotSupportedException();
        }

        public StackItem ToStackItem(ReferenceCounter referenceCounter)
        {
            return new Array(referenceCounter)
            {
                ScriptHash.ToArray(),
                EventName,
                State
            };
        }
    }
}
