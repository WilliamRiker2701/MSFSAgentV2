//=================================================================================================================
// PROJECT: MSFS Agent V2
// PURPOSE: This class adds Microsoft Flight Simulator 2020 agent support for Voice Attack (https://voiceattack.com/)
// AUTHOR: James Clark and William Riker
// Licensed under the MS-PL license. See LICENSE.md file in the project root for full license information.
//================================================================================================================= 

using System;
using System.Threading;
using System.Diagnostics;
using WASimCommander.CLI.Enums;
using WASimCommander.CLI.Structs;
using WASimCommander.Client;
using WASimCommander.Enums;
using WASimCommander.CLI.Client;
using WASimCommander.CLI;
using System.Runtime.Remoting.Contexts;
using static System.Net.Mime.MediaTypeNames;

namespace MSFS
{
    public class VoiceAttackPlugin
    {
        const string VARIABLE_NAMESPACE = "MSFSAgent";
        const string LOG_PREFIX = "MSFS Agent: ";
        const string LOG_NORMAL = "purple";
        const string LOG_ERROR = "red";
        const string LOG_INFO = "grey";

        /// <summary>
        /// Name of the plug-in as it should be shown in the UX
        /// </summary>
        public static string VA_DisplayName()
        {
            return "MSFS Agent - v2.0";
        }

        /// <summary>
        /// Extra information to display about the plug-in
        /// </summary>
        /// <returns></returns>
        public static string VA_DisplayInfo()
        {
            return "Enables access to Microsoft Flight Simulator game information and ability to trigger in game actions/events.";
        }

        /// <summary>
        /// Uniquely identifies the plugin
        /// </summary>
        public static Guid VA_Id()
        {
            return new Guid("{13CF33A1-302E-4D94-A02F-9A95C2E2A737}");
        }

        /// <summary>
        /// Used to stop any long running processes inside the plugin
        /// </summary>
        public static void VA_StopCommand()
        {
            // plugin has no long running processes
        }

        /// <summary>
        /// Runs when Voice Attack loads and processes plugins (runs once when the app launches)
        /// </summary>
        public static void VA_Init1(dynamic vaProxy)
        {

            // uncomment this line to force the debugger to attach at the very start of the class being created
            //System.Diagnostics.Debugger.Launch();

            if (!SupportedProfile(vaProxy)) return;

        }

        /// <summary>
        /// Handles clean up before Voice Attack closes
        /// </summary>
        public static void VA_Exit1(dynamic vaProxy)
        {
            // no clean up needed
        }

        /// <summary>
        /// Main function used to process commands from Voice Attack
        /// </summary>
        public static void VA_Invoke1(dynamic vaProxy)
        {
            string context;
            EventTypes requestedEvent;
            string eventData;
            Agent msfsAgent;
            string varKind = "Regular";
            string varAction = "Set";

            if (!SupportedProfile(vaProxy)) return;

            if (String.IsNullOrEmpty(vaProxy.Context))
                return;
            context = vaProxy.Context;
            if (context.Substring(0, 2) == "L:")
            {

                varKind = "L";

                if (context.Substring(context.Length - 2) == "-G")
                {

                    varAction = "Get";

                }
            }
            if (context.Substring(0, 2) == "P:")
            {

                varKind = "PMDG";

            }

            switch (varKind)
            {
                case "L":

                    switch (varAction)
                    {
                        case "Set":

                            msfsAgent = ConnectToWASM(vaProxy);

                            if (msfsAgent == null) return;

                            context = context.Remove(0, 2);

                            if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Processing L variable: " + context, LOG_INFO);

                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");

                            msfsAgent.TriggerWASM(context, eventData);

                            vaProxy.WriteToLog(LOG_PREFIX + "Context processed: " + context, LOG_NORMAL);

                            msfsAgent.WASMDisconnect();

                            break;

                        case "Get":

                            double varResult;

                            msfsAgent = ConnectToWASM(vaProxy);

                            if (msfsAgent == null) return;

                            context = context.Remove(0, 2);

                            context = context.Remove(context.Length - 2);

                            if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Processing L variable data request: " + context, LOG_INFO);
                                                     
                            varResult = msfsAgent.TriggerWASM(context);

                            vaProxy.SetDecimal(VARIABLE_NAMESPACE + ".PlaneState.Alt_Variable", (decimal?)(double?)varResult);

                            string calcCode = "(L:" + context + ")";

                            if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Calculator code " + calcCode + " returned: " + varResult, LOG_INFO);

                            vaProxy.WriteToLog(LOG_PREFIX + "Variable status: " + varResult, LOG_NORMAL);

                            msfsAgent.WASMDisconnect();

                            break;

                    }

                    break;

                case "PMDG":

                    msfsAgent = ConnectToSim(vaProxy);

                    if (msfsAgent == null) return;

                    context = context.Remove(0, 2);

                    if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Processing PMDG variable: " + context, LOG_INFO);

                    eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");

                    msfsAgent.TriggerPMDG(context, eventData);

                    vaProxy.WriteToLog(LOG_PREFIX + "Context processed: " + context, LOG_NORMAL);

                    msfsAgent.Disconnect();

                    break;

                case "Regular":

                    context = context.ToUpper();

                    msfsAgent = ConnectToSim(vaProxy);

                    if (msfsAgent == null) return;

                    if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Processing context: " + context, LOG_INFO);
                    switch (context)
                    {
                        case "GETPLANESTATE":
                            GetPlaneData(vaProxy, msfsAgent);
                            break;

                        case "ADF_COMPLETE_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.ADF_COMPLETE_SET, eventData);
                            break;

                        case "ADF2_COMPLETE_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.ADF2_COMPLETE_SET, eventData);
                            break;

                        case "AP_ALT_VAR_SET_ENGLISH":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.AP_ALT_VAR_SET_ENGLISH, eventData);
                            break;

                        case "AP_NAV_SELECT_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.AP_NAV_SELECT_SET, eventData);
                            break;

                        case "AP_SPD_VAR_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.AP_SPD_VAR_SET, eventData);
                            break;

                        case "AP_VS_VAR_SET_ENGLISH":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.AP_VS_VAR_SET_ENGLISH, eventData);
                            break;

                        case "BLEED_AIR_SOURCE_CONTROL_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.BLEED_AIR_SOURCE_CONTROL_SET, eventData);
                            break;

                        case "COM_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.COM_RADIO_SET, eventData);
                            break;

                        case "COM_STBY_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.COM_STBY_RADIO_SET, eventData);
                            break;

                        case "COM2_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.COM2_RADIO_SET, eventData);
                            break;

                        case "COM2_STBY_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.COM2_STBY_RADIO_SET, eventData);
                            break;

                        case "HEADING_BUG_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.HEADING_BUG_SET, eventData);
                            break;

                        case "NAV1_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.NAV1_RADIO_SET, eventData);
                            break;

                        case "NAV1_STBY_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.NAV1_STBY_SET, eventData);
                            break;

                        case "NAV2_RADIO_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.NAV2_RADIO_SET, eventData);
                            break;

                        case "NAV2_STBY_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.NAV2_STBY_SET, eventData);
                            break;

                        case "XPNDR_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.XPNDR_SET, eventData);
                            break;

                        case "FLAPS_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.FLAPS_SET, eventData);
                            break;

                        case "SPOILERS_ARM_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.SPOILERS_ARM_SET, eventData);
                            break;

                        case "AP_MAX_BANK_SET":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.AP_MAX_BANK_SET, eventData);
                            break;

                        case "FUELSYSTEM_PUMP_TOGGLE":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.FUELSYSTEM_PUMP_TOGGLE, eventData);
                            break;

                        case "ELECTRICAL_CIRCUIT_TOGGLE":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.ELECTRICAL_CIRCUIT_TOGGLE, eventData);
                            break;

                        case "TURBINE_IGNITION_SWITCH_SET1":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.TURBINE_IGNITION_SWITCH_SET1, eventData);
                            break;

                        case "TURBINE_IGNITION_SWITCH_SET2":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.TURBINE_IGNITION_SWITCH_SET2, eventData);
                            break;

                        case "FUELSYSTEM_VALVE_OPEN":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.FUELSYSTEM_VALVE_OPEN, eventData);
                            break;

                        case "FUELSYSTEM_VALVE_CLOSE":
                            eventData = vaProxy.GetText(VARIABLE_NAMESPACE + ".EventData");
                            msfsAgent.TriggerEvent(EventTypes.FUELSYSTEM_VALVE_CLOSE, eventData);
                            break;


                        default:

                            // for all other cases, pass the "context" command through to the agent as a requested event with no data

                            try
                            {
                                // parse the context to find a matching event in the EventTypes enum, if matched, we'll request it
                                requestedEvent = (EventTypes)Enum.Parse(typeof(EventTypes), context, false);
                                msfsAgent.TriggerEvent(requestedEvent);
                            }
                            catch
                            {
                                vaProxy.WriteToLog(LOG_PREFIX + "Unknown context sent to agent: " + context, LOG_ERROR);
                                msfsAgent.Disconnect();
                                return;
                            }
                            break;

                    }
                    // if we get this far we've processed the command

                    vaProxy.WriteToLog(LOG_PREFIX + "Context processed: " + context, LOG_NORMAL);

                    msfsAgent.Disconnect();


                    break;

            }

        }

        /// <summary>
        /// Looks for a VA variable to determine if additional logging should be done to the main VA window
        /// </summary>
        private static bool DebugMode(dynamic vaProxy)
        {
            // enables more detailed logging
            bool? result = vaProxy.GetBoolean(VARIABLE_NAMESPACE + ".DebugMode");
            if (result.HasValue)
                return result.Value;
            else
                return false;
        }

        /// <summary>
        /// Checks to see if the agent should run for the current Voice Attack profile or otherwise sleep
        /// </summary>
        private static bool SupportedProfile(dynamic vaProxy)
        {

            //TODO: implement better method to determine if the profile is a matching one

            if (!vaProxy.Command.Exists("MSFS Agent"))
            {
                if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "'MSFS Agent' command not found in this profile.  Agent standing down.", LOG_INFO);
                return false;
            }
            else
            {
                if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Profile is a match.  Agent enabled.", LOG_INFO);
                return true;
            }

        }

        /// <summary>
        /// Creates a connection to the sim
        /// </summary>
        private static Agent ConnectToSim(dynamic vaProxy)
        {

            Agent msfsAgent = new Agent();
            msfsAgent.Connect();

            if (msfsAgent.Connected)
            {
                // need to make sure data definitions are loaded otherwise we won't get structured data back from the sim
                msfsAgent.AddDataDefinitions();

                if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Successfully connected to SimConnect.", LOG_INFO);
                return msfsAgent;
            }
            else
            {
                if (msfsAgent != null) msfsAgent.Disconnect();
                vaProxy.WriteToLog(LOG_PREFIX + "Couldn't make a connection to SimConnect.", LOG_ERROR);
                return null;
            }



        }


        private static Agent ConnectToWASM(dynamic vaProxy)
        {

            Agent msfsAgent = new Agent();
            msfsAgent.WASMConnect();

            if (msfsAgent.WAServerConnected)
            {

                if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Successfully connected to WASM.", LOG_INFO);
                return msfsAgent;
            }
            else
            {
                if (msfsAgent != null) msfsAgent.WASMDisconnect();
                vaProxy.WriteToLog(LOG_PREFIX + "Couldn't make a connection to SimConnect.", LOG_ERROR);
                return null;
            }


        }

        /// <summary>
        /// Gets data from the sim and exposes the data as variables in Voice Attack
        /// </summary>
        private static void GetPlaneData(dynamic vaProxy, Agent msfsAgent)
        {

            if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "GetPlaneData action called.", LOG_INFO);


            // see if we still have a valid connection
            if (msfsAgent == null || !msfsAgent.Connected)
            {
                vaProxy.WriteToLog(LOG_PREFIX + "GetPlaneData failed. No connection to sim.", LOG_ERROR);
                return;
            }

            try
            {
                // make the request to get new data from the sim
                msfsAgent.RequestData(RequestTypes.PlaneState, DataDefinitions.PlaneState);

                while (msfsAgent.RequestPending(RequestTypes.PlaneState))
                {
                    System.Threading.Thread.Sleep(10);
                    msfsAgent.CheckForMessage();
                }

            }
            catch (Exception ex)
            {
                vaProxy.WriteToLog(LOG_PREFIX + "GetPlaneData failed.  Msg:" + ex.Message, LOG_ERROR);
                return;
            }


            try
            {
                // loops through the returned data structure and converts each field into a voice attack variable
                foreach (var field in typeof(PlaneState).GetFields())
                {
                    if (field.FieldType == typeof(Boolean))
                        vaProxy.SetBoolean(VARIABLE_NAMESPACE + ".PlaneState." + field.Name, (bool?)field.GetValue(msfsAgent.GetPlaneState));
                    if (field.FieldType == typeof(String))
                        vaProxy.SetText(VARIABLE_NAMESPACE + ".PlaneState." + field.Name, (string)field.GetValue(msfsAgent.GetPlaneState));
                    if (field.FieldType == typeof(Int32))
                        vaProxy.SetInt(VARIABLE_NAMESPACE + ".PlaneState." + field.Name, (int?)field.GetValue(msfsAgent.GetPlaneState));
                    if (field.FieldType == typeof(Double))
                        vaProxy.SetDecimal(VARIABLE_NAMESPACE + ".PlaneState." + field.Name, (decimal?)(double?)field.GetValue(msfsAgent.GetPlaneState));
                }
            }
            catch (Exception ex)
            {
                vaProxy.WriteToLog(LOG_PREFIX + "GetPlaneData failed to convert sim data into voice attack variables.  Msg:" + ex.Message, LOG_ERROR);
                return;
            }


            if (DebugMode(vaProxy)) vaProxy.WriteToLog(LOG_PREFIX + "Data sucessfully updated.", LOG_INFO);
        }

    }
}


