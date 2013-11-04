//
// --------------------------------------------------------------------------
//  Gurux Ltd
// 
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License 
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
// See the GNU General Public License for more details.
//
// More information of Gurux products: http://www.gurux.org
//
// This code is licensed under the GNU General Public License v2. 
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using Gurux.DLMS;
using System.Xml.Serialization;
using Gurux.DLMS.ManufacturerSettings;
using Gurux.DLMS.Internal;

namespace Gurux.DLMS.Objects
{

    public enum ClockStatus
    {
        Ok = 0,
        InvalidValue = 0x1,
        DoubtfulValue = 0x2,
        DifferentClockBase = 0x4,
        Reserved1 = 0x8,
        Reserved2 = 0x10,
        Reserved3 = 0x20,
        Reserved4 = 0x40,
        DaylightSaveActive = 0x40
    }

    public class GXDLMSClock : GXDLMSObject, IGXDLMSBase
    {
        /// <summary> 
        /// Constructor.
        /// </summary> 
        public GXDLMSClock()
            : base(ObjectType.Clock, "0.0.1.0.0.255", 0)
        {
        }

        public override DataType GetUIDataType(int index)
        {
            if (index == 2)
            {
                return DataType.DateTime;
            }
            return base.GetUIDataType(index);
        }

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logican Name of the object.</param>
        public GXDLMSClock(string ln)
            : base(ObjectType.Clock, ln, 0)
        {
        }

        /// <summary> 
        /// Constructor.
        /// </summary> 
        /// <param name="ln">Logican Name of the object.</param>
        /// <param name="sn">Short Name of the object.</param>
        public GXDLMSClock(string ln, ushort sn)
            : base(ObjectType.Clock, ln, sn)
        {
        }

        /// <inheritdoc cref="GXDLMSObject.LogicalName"/>
        [DefaultValue("0.0.1.0.0.255")]
        override public string LogicalName
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.UpdateDefaultValueItems"/>
        public override void UpdateDefaultValueItems()
        {
            SetDataType(2, DataType.DateTime);
            GXDLMSAttributeSettings att = this.Attributes.Find(4);
            if (att == null)
            {
                att = new GXDLMSAttribute(4);
                att.Access = AccessMode.Read;
                att.Name = "Status";
                Attributes.Add(att);
            }
        }

        /// <summary>
        /// Time of COSEM Clock object.
        /// </summary>
        [XmlIgnore()]
        public GXDateTime Time
        {
            get;
            set;
        }

        /// <summary>
        /// TimeZone of COSEM Clock object.
        /// </summary>
        [XmlIgnore()]
        public int TimeZone
        {
            get;
            set;
        }

        /// <summary>
        /// Status of COSEM Clock object.
        /// </summary>
        [XmlIgnore()]
        public ClockStatus Status
        {
            get;
            set;
        }

        [XmlIgnore()]
        public GXDateTime Begin
        {
            get;
            set;
        }

        [XmlIgnore()]
        public GXDateTime End
        {
            get;
            set;
        }

        [XmlIgnore()]
        public int Deviation
        {
            get;
            set;
        }

        [XmlIgnore()]
        public bool Enabled
        {
            get;
            set;
        }

        /// <summary>
        /// Clock base of COSEM Clock object.
        /// </summary>
        [XmlIgnore()]
        public int ClockBase
        {
            get;
            set;
        }

        /// <inheritdoc cref="GXDLMSObject.GetValues"/>
        public override object[] GetValues()
        {
            return new object[] { LogicalName, Time, TimeZone, Status, Begin, End, 
                Deviation, Enabled, ClockBase };
        }

        #region IGXDLMSBase Members

        byte[] IGXDLMSBase.Invoke(object sender, int index, Object parameters)
        {
            DateTime tm = this.Time.Value;
            // Resets the value to the default value. 
            // The default value is an instance specific constant.
            if (index == 1)
            {                
                int minutes = tm.Minute;
                if (minutes < 8)
                {
                    minutes = 0;
                }
                else if (minutes < 23)
                {
                    minutes = 15;
                }
                else if (minutes < 38)
                {
                    minutes = 30;
                }
                else if (minutes < 53)
                {
                    minutes = 45;
                }
                else
                {
                    minutes = 0;
                    tm = tm.AddHours(1);
                }
                tm = tm.AddMinutes(-tm.Minute + minutes);
                tm = tm.AddSeconds(-tm.Second);
                tm = tm.AddMilliseconds(-tm.Millisecond);
                this.Time.Value = tm;
            }
            // Sets the meter�s time to the nearest minute.
            else if (index == 3)
            {                
                tm = this.Time.Value;
                int s = tm.Second;
                if (s > 30)
                {
                    tm = tm.AddMinutes(1);
                }
                tm = tm.AddSeconds(-tm.Second);
                tm = tm.AddMilliseconds(-tm.Millisecond);
                this.Time.Value = tm;
            }
            // Presets the time to a new value (preset_time) and defines 
            // avalidity_interval within which the new time can be activated.
            else if (index == 5)
            {
                GXDateTime presetTime = (GXDateTime)GXDLMSClient.ChangeType((byte[])((Object[])parameters)[0], DataType.DateTime);
                GXDateTime validityIntervalStart = (GXDateTime)GXDLMSClient.ChangeType((byte[])((Object[])parameters)[1], DataType.DateTime);
                GXDateTime validityIntervalEnd = (GXDateTime)GXDLMSClient.ChangeType((byte[])((Object[])parameters)[2], DataType.DateTime);
                this.Time.Value = presetTime.Value;
            }
            // Shifts the time.
            else if (index == 6)
            {
                int shift = Convert.ToInt32(parameters);
                tm = tm.AddSeconds(shift);
                this.Time.Value = tm;
            }
            else
            {
                throw new ArgumentException("Invoke failed. Invalid attribute index.");
            }
            return null;
        }

        /// <summary>
        /// Sets the meter�s time to the nearest (+/-) quarter of an hour value (*:00, *:15, *:30, *:45).
        /// </summary>
        /// <returns></returns>
        public byte[][] AdjustToQuarter(GXDLMSClient client)
        {
            byte[] ret = client.Method(this, 1, (int)0);
            return new byte[][] { ret };
        }


        /// <summary>
        /// Sets the meter�s time to the nearest (+/-) starting point of a measuring period.
        /// </summary>
        /// <returns></returns>
        public byte[][] AdjustToMeasuringPeriod(GXDLMSClient client)
        {
            byte[] ret = client.Method(this, 2, (int)0);
            return new byte[][] { ret };
        }

        /// <summary>
        /// Sets the meter�s time to the nearest minute.
        /// If second_counter < 30 s, so second_counter is set to 0.
        /// If second_counter � 30 s, so second_counter is set to 0, and
        /// minute_counter and all depending clock values are incremented if necessary.
        /// </summary>
        /// <returns></returns>
        public byte[][] AdjustToMinute(GXDLMSClient client)
        {
            byte[] ret = client.Method(this, 3, (int)0);
            return new byte[][] { ret };
        }

        /// <summary>
        /// This Method is used in conjunction with the preset_adjusting_time
        /// Method. If the meter�s time lies between validity_interval_start and
        /// validity_interval_end, then time is set to preset_time.
        /// </summary>
        /// <returns></returns>
        public byte[][] AdjustToPresetTime(GXDLMSClient client)
        {
            byte[] ret = client.Method(this, 4, (int)0);
            return new byte[][] { ret };
        }

        /// <summary>
        /// Presets the time to a new value (preset_time) and defines a validity_interval within which the new time can be activated.
        /// </summary>
        /// <param name="presetTime"></param>
        /// <param name="validityIntervalStart"></param>
        /// <param name="validityIntervalEnd"></param>
        /// <returns></returns>
        public byte[][] PresetAdjustingTime(GXDLMSClient client, DateTime presetTime, DateTime validityIntervalStart, DateTime validityIntervalEnd)
        {
            List<byte> buff = new List<byte>();
            buff.Add((byte)DataType.Structure);
            buff.Add((byte)3);
            GXCommon.SetData(buff, DataType.DateTime, presetTime);
            GXCommon.SetData(buff, DataType.DateTime, validityIntervalStart);
            GXCommon.SetData(buff, DataType.DateTime, validityIntervalEnd);
            byte[] ret = client.Method(this, 5, buff.ToArray(), DataType.Array);
            return new byte[][] { ret };
        }

        /// <summary>
        /// Shifts the time by n (-900 <= n <= 900) s.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public byte[][] ShiftTime(GXDLMSClient client, int time)
        {
            if (time < -900 || time > 900)
            {
                throw new ArgumentOutOfRangeException("Invalid shift time.");
            }
            byte[] ret = client.Method(this, 6, time);
            return new byte[][] { ret };
        }

        int[] IGXDLMSBase.GetAttributeIndexToRead()
        {
            List<int> attributes = new List<int>();
            //LN is static and read only once.
            if (string.IsNullOrEmpty(LogicalName))
            {
                attributes.Add(1);
            }
            //Time
            if (CanRead(2))
            {
                attributes.Add(2);
            }
            //TimeZone
            if (!base.IsRead(3))
            {
                attributes.Add(3);
            }
            //Status
            if (CanRead(4))
            {
                attributes.Add(4);
            }
            //Begin
            if (!base.IsRead(5))
            {
                attributes.Add(5);
            }
            //End
            if (!base.IsRead(6))
            {
                attributes.Add(6);
            }
            //Deviation
            if (!base.IsRead(7))
            {
                attributes.Add(7);
            }
            //Enabled
            if (!base.IsRead(8))
            {
                attributes.Add(8);
            }
            //ClockBase
            if (!base.IsRead(9))
            {
                attributes.Add(9);
            }
            return attributes.ToArray();
        }

        int IGXDLMSBase.GetAttributeCount()
        {
            return 9;
        }

        int IGXDLMSBase.GetMethodCount()
        {
            return 6;
        }

        object IGXDLMSBase.GetValue(int index, out DataType type, byte[] parameters, bool raw)
        {
            if (index == 1)
            {
                type = DataType.OctetString;
                return GXDLMSObject.GetLogicalName(this.LogicalName);
            }
            if (index == 2)
            {
                type = DataType.DateTime;
                return Time;
            }
            if (index == 3)
            {
                type = DataType.Int16;
                return TimeZone;
            }
            if (index == 4)
            {
                type = DataType.UInt8;
                return Status;
            }
            if (index == 5)
            {
                type = DataType.DateTime;
                return Begin;
            }
            if (index == 6)
            {
                type = DataType.DateTime;
                return End;
            }
            if (index == 7)
            {
                type = DataType.Int8;
                return Deviation;
            }
            if (index == 8)
            {
                type = DataType.Boolean;
                return Enabled;
            }
            if (index == 9)
            {
                type = DataType.Enum;
                return ClockBase;
            }
            throw new ArgumentException("GetValue failed. Invalid attribute index.");
        }

        void IGXDLMSBase.SetValue(int index, object value, bool raw)
        {
            if (index == 1)
            {
                if (value is string)
                {
                    LogicalName = value.ToString();
                }
                else
                {
                    LogicalName = GXDLMSClient.ChangeType((byte[])value, DataType.OctetString).ToString();
                }                
            }
            else if (index == 2)
            {
                if (value == null)
                {
                    Time = new GXDateTime(DateTime.MinValue);
                }
                else
                {
                    if (value is byte[])
                    {
                        value = GXDLMSClient.ChangeType((byte[]) value, DataType.DateTime);
                    }                    
                    Time = (GXDateTime)value;
                }
            }
            else if (index == 3)
            {
                TimeZone = Convert.ToInt32(value);
            }
            else if (index == 4)
            {
                Status = (ClockStatus)Convert.ToInt32(value);
            }
            else if (index == 5)
            {
                if (value == null)
                {
                    Begin = new GXDateTime(DateTime.MinValue);
                }
                else
                {
                    if (value is byte[])
                    {
                        value = GXDLMSClient.ChangeType((byte[])value, DataType.DateTime);
                    }
                    Begin = (GXDateTime)value;
                }                
            }
            else if (index == 6)
            {
                if (value == null)
                {
                    End = new GXDateTime(DateTime.MinValue);
                }
                else
                {
                    if (value is byte[])
                    {
                        value = GXDLMSClient.ChangeType((byte[])value, DataType.DateTime);
                    }
                    End = (GXDateTime)value;
                }                
            }
            else if (index == 7)
            {
                Deviation = Convert.ToInt32(value);
            }
            else if (index == 8)
            {
                Enabled = Convert.ToBoolean(value);
            }
            else if (index == 9)
            {
                ClockBase = Convert.ToInt32(value);
            }
            else
            {
                throw new ArgumentException("SetValue failed. Invalid attribute index.");
            }
        }
        #endregion
    }
}
