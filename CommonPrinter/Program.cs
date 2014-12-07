using System;
using System.Collections.Generic;
using DarkMultiPlayerCommon;

namespace CommonPrinter
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			String filename = "common.py";
			// New file, or blank out the previous one
			NewBlankFile (filename, "from enum import Enum");
			FileBlankLine (filename);
			FileBlankLine (filename);

			StartPythonClass (filename, "Protocol", "");
			StartPythonInit (filename);
			// Add vars from consts
			ConstToPythonClassVar<long> (filename, "HEART_BEAT_INTERVAL");
			ConstToPythonClassVar<long> (filename, "INITIAL_CONNECTION_TIMEOUT");
			ConstToPythonClassVar<long> (filename, "CONNECTION_TIMEOUT");
			ConstToPythonClassVar<int> (filename, "MAX_MESSAGE_SIZE");
			ConstToPythonClassVar<int> (filename, "SPLIT_MESSAGE_LENGTH");
			ConstToPythonClassVar<int> (filename, "PROTOCOL_VERSION");
			ConstToPythonClassVar<String> (filename, "PROGRAM_VERSION");

			EndPythonClass (filename);

			// Add enums
			EnumToPython (filename, "CraftType", GetCommonTypes<CraftType> ());
			EnumToPython (filename, "ClientMessageType", GetCommonTypes<ClientMessageType> ());
			EnumToPython (filename, "ServerMessageType", GetCommonTypes<ServerMessageType> ());
			EnumToPython (filename, "ConnectionStatus", GetCommonTypes<ConnectionStatus> ());
			EnumToPython (filename, "ClientState", GetCommonTypes<ClientState> ());
			EnumToPython (filename, "WarpMode", GetCommonTypes<WarpMode> ());
			EnumToPython (filename, "GameMode", GetCommonTypes<GameMode> ()); 
			EnumToPython (filename, "ModControlMode", GetCommonTypes<ModControlMode> ());
			EnumToPython (filename, "WarpMessageType", GetCommonTypes<WarpMessageType> ());
			EnumToPython (filename, "CraftMessageType", GetCommonTypes<CraftMessageType> ());
			EnumToPython (filename, "ScreenshotMessageType", GetCommonTypes<ScreenshotMessageType> ());
			EnumToPython (filename, "ChatMessageType", GetCommonTypes<ChatMessageType> ());
			EnumToPython (filename, "AdminMessageType", GetCommonTypes<AdminMessageType> ());
			EnumToPython (filename, "LockMessageType", GetCommonTypes<LockMessageType> ());
			EnumToPython (filename, "FlagMessageType", GetCommonTypes<FlagMessageType> ()); 
			EnumToPython (filename, "PlayerColorMessageType", GetCommonTypes<PlayerColorMessageType> ());
			EnumToPython (filename, "HandshakeReply", GetCommonTypes<HandshakeReply> ());
		}

		public static List<String> GetCommonTypes<MessageEnum>()
		{
			List<String> res = new List<String>();
			var cmtList = Enum.GetValues (typeof(MessageEnum));

			foreach (MessageEnum messagetype in cmtList)
			{

				var code = Convert.ChangeType (messagetype, Type.GetTypeCode(messagetype.GetType()));
				res.Add(messagetype.ToString() + " = " +  code.ToString());
			}

			return res;
		}

		public static void NewBlankFile(String output, String firstLine)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, false))
			{
				file.WriteLine (firstLine);
			}
		}

		public static void FileBlankLine(String output)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				file.WriteLine ();
			}
		}

		public static void EnumToPython(String output, String className, List<String> classMembers)
		{
			StartPythonClass (output, className, "Enum");
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				foreach (String classEntry in classMembers)
				{
					// Write the line
					file.WriteLine ("    " + classEntry);
				}
			}
			EndPythonClass (output);
		}

		public static void ConstToPythonClassVar<T>(String output, String constname)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				T cconst = (T)typeof(DarkMultiPlayerCommon.Common).GetField(constname).GetValue(null);

				if (typeof(T) == typeof(String))
				{
					file.WriteLine ("        self." + constname + " = \"" + cconst.ToString () + "\"");
				}
				else
				{
					file.WriteLine ("        self." + constname + " = " + cconst.ToString ());
				}
			}
		}

		public static void StartPythonClass(String output, String className, String inherit)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				// Write the line
				file.WriteLine ("class " + className + "(" + inherit +"):");
			}
		}

		public static void StartPythonInit(String output)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				// Write the line
				file.WriteLine ("    def __init__(self):");
			}
		}

		public static void EndPythonClass(String output)
		{
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(output, true))
			{
				// Write 2 lines for proper spacing
				file.WriteLine ();
				file.WriteLine ();
			}
		}
	}
}

