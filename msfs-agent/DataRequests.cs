//=================================================================================================================
// PROJECT: MSFS Agent V2
// PURPOSE: This file defines the data structures used to request data from the sim.
// AUTHOR: James Clark and William Riker
// Licensed under the MS-PL license. See LICENSE.md file in the project root for full license information.
//================================================================================================================= 
using System.Runtime.InteropServices;

namespace MSFS
{
    //Defines the notification groups we want to use
    public enum NOTIFICATION_GROUPS
    {
        DEFAULT
    }

    public enum hSimconnect : int
    {
        group1
    }

    // Identifies the types of data requests we want to make
    public enum RequestTypes
    {
        PlaneState,
    }

    // Identifies the different data definitions we've defined
    public enum DataDefinitions
    {
        PlaneState,
    }


    // We need data structure for each data request
    // Note: each string in the data structure needs a MarshalAs statement above it
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct PlaneState
    {
        
        public int Airspeed_Indicated;
        public int Ambient_Temperature;
        public bool Apu_Switch;
        public bool Apu_Generator_Switch;
        public double Apu_Pct_Rpm;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Atc_Airline;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Atc_Flight_Number;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Atc_Id;
        public double Auto_Brake_Switch_CB;
        public bool Autopilot_Airspeed_Hold;
        public int Autopilot_Airspeed_Hold_Var;
        public bool Autopilot_Altitude_Lock;
        public int Autopilot_Altitude_Lock_Var;
        public bool Autopilot_Approach_Hold;
        public bool Autopilot_Attitude_Hold;
        public bool Autopilot_Available;
        public bool Autopilot_Backcourse_Hold;
        public bool Autopilot_Disengaged;
        public bool Autopilot_Flight_Director_Active;
        public bool Autopilot_Heading_Lock;
        public int Autopilot_Heading_Lock_Dir;
        public bool Autopilot_Master;
        public int Autopilot_Max_Bank_ID;
        public double Autopilot_Nav_Selected;
        public bool Autopilot_Nav1_Lock;
        public bool Autopilot_Throttle_Arm;
        public bool Autopilot_Vertical_Hold;
        public int Autopilot_Vertical_Hold_Var;
        public bool Autopilot_Yaw_Damper;
        public int Bleed_Air_Source_Control;
        public bool Brake_Parking_Indicator;
        public bool Circuit_Switch_On_17;
        public bool Circuit_Switch_On_18;
        public bool Circuit_Switch_On_19;
        public bool Circuit_Switch_On_20;
        public bool Circuit_Switch_On_21;
        public bool Circuit_Switch_On_22;
        public double Com1_Active_Frequency;
        public double Com1_Standby_Frequency;
        public double Com2_Active_Frequency;
        public double Com2_Standby_Frequency;
        public bool Electrical_Master_Battery;
        public bool External_Power_On;
        public bool Engine_Anti_Ice_1;
        public bool Engine_Anti_Ice_2;
        public double Engine_N1_RPM_1;
        public double Engine_N1_RPM_2;
        public double Engine_N2_RPM_1;
        public double Engine_N2_RPM_2;
        public int Engine_Type;
        public double Flaps_Handle_Index;
        public double Flaps_Handle_Percent;
        public bool Fuelsystem_Pump_Switch_1;
        public bool Fuelsystem_Pump_Switch_2;
        public bool Fuelsystem_Pump_Switch_3;
        public bool Fuelsystem_Pump_Switch_4;
        public bool Fuelsystem_Pump_Switch_5;
        public bool Fuelsystem_Pump_Switch_6;
        public bool Fuelsystem_Valve_Switch_1;
        public bool Fuelsystem_Valve_Switch_2;
        public bool Fuelsystem_Valve_Switch_3;
        public int Gear_Handle_Position;
        public bool General_Eng_Starter_1;
        public bool General_Eng_Starter_2;
        public int Ground_Velocity;
        public int Heading_Indicator;
        public bool Hydraulic_Switch;
        public bool Is_Gear_Retractable;
        public bool Light_Beacon;
        public bool Light_Cabin;
        public bool Light_Landing;
        public bool Light_Logo;
        public bool Light_Nav;
        public bool Light_Panel;
        public bool Light_Recognition;
        public bool Light_Strobe;
        public bool Light_Taxi;
        public bool Light_Wing;
        public int Local_Time;
        public bool Master_Ignition_Switch;
        public double Nav1_Active_Frequency;
        public double Nav1_Standby_Frequency;
        public double Nav2_Active_Frequency;
        public double Nav2_Standby_Frequency;
        public double Number_Of_Engines;
        public bool Panel_Anti_Ice_Switch;
        public bool Pitot_Heat;
        public int Plane_Alt_Above_Ground;
        public int Plane_Altitude;
        public double Plane_Latitude;
        public double Plane_Longitude;
        public bool Prop_Deice_Switch;
        public int Pushback_State;
        public bool Sim_On_Ground;
        public bool Spoiler_Available;
        public double Spoilers_Handle_Position;
        public bool Structural_Deice_Switch;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Title;
        public bool Transponder_Available;
        public int Transponder_Code;
        public int Vertical_Speed;
        public int Water_Rudder_Handle_Position;
        public bool Windshield_Deice_Switch;
        public double Alt_Variable;
    }

}
