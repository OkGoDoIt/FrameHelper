using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FrameHelper
{
	internal static class FrameLibrary
	{
		public static async Task<bool> InjectLibraryFunction(this Frame frame, string name, string scriptContents)
		{
			string exisits = await frame.PrintShortLuaResponse($"tostring({name} ~= nil)");
			if (exisits != "true")
			{
				Debug.Print(name + " does not exist, injecting it");
				if (!await frame.SendLuaChecked($"require(\"{name}\")", TimeSpan.FromSeconds(1)))
				{
					Debug.Print(name + ".lua does not exist, creating it");
					await frame.RunScriptViaFile(scriptContents, name);
					Debug.Print(name + " created");
				} else
				{
					Debug.Print(name + ".lua exists, required it");
				}
				exisits = await frame.PrintShortLuaResponse($"tostring({name} ~= nil)");
				if (exisits != "true") {
					Debug.Print(name + " failed to inject");
					return false;
				}
			}
			return exisits == "true";
		}

		public static async Task<bool> InjectAllLibraryFunctions(this Frame frame)
		{
			bool success = true;
			success &= await InjectLibraryFunction(frame, "prntLng", longReturnHelperScript);
			success &= await InjectLibraryFunction(frame, "clearDisplay", clearDisplayHelperScript);
			success &= await InjectLibraryFunction(frame, "listFilesInDir", listFilesInDirHelperScript);
			success &= await InjectLibraryFunction(frame, "readFullFile", readFullFileHelperScript);
			success &= await InjectLibraryFunction(frame, "sendDataInChunks", sendDataInChunksHelperScript);
			success &= await InjectLibraryFunction(frame, "sendData", sendDataHelperScript);
			return success;
		}

		private const string longReturnHelperScript = """
			local mtu = frame.bluetooth.max_length()
			function prntLng(id,stringToPrint)
				local len = string.len(stringToPrint)
				local i = 1
				local chunkIndex = 0
				while i <= len do
					local j = i + mtu - 25 - 1
					if j > len then
						j = len
					end
					local chunk = string.sub(stringToPrint, i, j)
					print('~'..id..'-'..chunkIndex..':'..chunk)
					chunkIndex = chunkIndex + 1
					i = j + 1
				end
				print('~'..id..'!'..chunkIndex)
			end
			""";

		private const string clearDisplayHelperScript = """
			function clearDisplay()
				frame.display.text(' ', 1, 1)
				frame.display.show()
			end
			""";

		private const string listFilesInDirHelperScript = """
			function listFilesInDir(dir)
				local files = frame.file.listdir(dir)
				local str = ''
				for i, data in ipairs(files) do
					for key, value in pairs(data) do
						if key == 'name' then
							str = str..value
						end
						if key == 'type' and value == 2 then
							str = str..'/'
						end
					end
					str = str..'\n'
				end
				return str
			end
			""";

		private const string readFullFileHelperScript = """
			function readFullFile(file)
				local f = frame.file.open(file)
				local str = ''
				while true do
					local chunk = f:read()
					if chunk == nil then
						break
					end
					str = str..chunk..'\n'
				end
				f:close()
				return str
			end
			""";

		private const string sendDataInChunksHelperScript = """
			function sendDataInChunks(type,isNew,data)
				if type < 0 or type > 127 then
					print('Invalid type')
					return
				end

				local mtu = frame.bluetooth.max_length()
				local len = string.len(data)
				local i = 1
				local chunkIndex = 0
				while i <= len do
					local chunk = string.sub(data, i, i+mtu-5)
					if (not sendData(type,isNew,chunk)) then
						return false
					end
					isNew = false
					chunkIndex = chunkIndex + 1
					i = i + mtu - 5					
				end
				if (not sendData(type,false,'')) then
					return false
				end
				return true
			end
			""";


		private const string sendDataHelperScript = """
			function sendData(type,isNew,data)
				if type < 0 or type > 127 then
					print('Invalid type')
					return
				end
				local typeId = type
				if isNew then
					typeId = typeId + 128
				end
				
				local try_until = frame.time.utc() + 2
				data = string.char(typeId) .. data

				while frame.time.utc() < try_until do
					if pcall(frame.bluetooth.send, data) then
						return true
					end
				end

				print('unable to send')
				return false
			end
			""";
	}
}
