using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.PointOfService;
using Windows.Storage.Streams;
using static FrameHelper.FrameBluetoothLEConnection;

namespace FrameHelper
{
	public class Frame
	{
		public FrameBluetoothLEConnection Connection
		{
			get; private set;
		}


		public Frame()
		{
			Connection = new FrameBluetoothLEConnection();
			Connection.OnReceiveData += Connection_OnReceiveData;
			//Connection.Connect();

		}

		private void Connection_OnReceiveData(FrameBluetoothLEConnection sender, string receivedString)
		{
			if (receivedString.StartsWith("~"))
			{
				if (receivedString[4] == ':')
				{
					string callbackId = receivedString.Substring(1, 3);
					string payload = receivedString.Substring(5);
					if (callbacks.ContainsKey(callbackId))
					{
						callbacks[callbackId](payload);
						callbacks.Remove(callbackId);
					}
					else
					{
						Debug.WriteLine($"Callback {callbackId} not found");
					}
				}
				else if (receivedString[4] == '.')
				{
					string callbackId = receivedString.Substring(1, 3);
					if (callbacks.ContainsKey(callbackId))
					{
						callbacks[callbackId](callbackId);
						callbacks.Remove(callbackId);
					}
					else
					{
						Debug.WriteLine($"Confirming callback {callbackId} not found");
					}
				}
				else if (receivedString[4] == '-')
				{
					string callbackId = receivedString.Substring(1, 3);
					int colonIndex = receivedString.IndexOf(':', 5);
					int callbackIndex = int.Parse(receivedString[5..colonIndex]);
					string payload = receivedString.Substring(colonIndex + 1);

					if (!payloads.ContainsKey(callbackId))
					{
						payloads.Add(callbackId, new List<string>());
					}
					if (payloads[callbackId].Count == callbackIndex)
					{
						payloads[callbackId].Add(payload);
					}
					else if (callbackIndex < payloads[callbackId].Count)
					{
						payloads[callbackId][callbackIndex] = payload;
					}
					else if (callbackIndex > payloads[callbackId].Count)
					{
						for (int i = payloads[callbackId].Count; i < callbackIndex; i++)
						{
							payloads[callbackId].Add(null);
						}
					}
				}
				else if (receivedString[4] == '!')
				{
					string callbackId = receivedString.Substring(1, 3);
					int totalChunks = int.Parse(receivedString.Substring(5));
					if (payloads.ContainsKey(callbackId))
					{
						if (payloads[callbackId].Count == totalChunks)
						{
							string payload = string.Join("", payloads[callbackId]);
							if (callbacks.ContainsKey(callbackId))
							{
								callbacks[callbackId](payload);
								callbacks.Remove(callbackId);
							}
							else
							{
								Debug.WriteLine($"Callback for {callbackId} not found");
							}
							payloads.Remove(callbackId);
						}
						else
						{
							Debug.WriteLine($"Expected {totalChunks} chunks, but got {payloads[callbackId].Count}");
						}
					}
					else
					{
						Debug.WriteLine($"Extended callback {callbackId} not found");
					}
				}
			}
			else if (_onReceive != null)
			{
				_onReceive(receivedString);
			}
		}

		public Frame(FrameBluetoothLEConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this.Connection = connection;
		}

		float? cachedBatteryLevel = null;
		DateTimeOffset whenBatteryLevelCached = DateTimeOffset.MinValue;
		public async Task<float?> GetBatteryLevelAsync()
		{
			if (cachedBatteryLevel != null && DateTimeOffset.Now - whenBatteryLevelCached < TimeSpan.FromSeconds(30))
			{
				return cachedBatteryLevel;
			}

			string result = null;

			if (Connection.MTUSize > 100)
				result = await PrintShortLuaResponse("frame.battery_level()", TimeSpan.FromSeconds(1));
			else
			{
				if (await SendLua("fr=frame"))
					if (await SendLua("bl=fr.battery_level"))
					{
						result = await PrintShortLuaResponse("bl()", TimeSpan.FromSeconds(1));
					}
			}
			if (result != null && float.TryParse(result, out float batteryLevel))
			{
				cachedBatteryLevel = batteryLevel;
				whenBatteryLevelCached = DateTimeOffset.Now;
				return batteryLevel;
			}
			cachedBatteryLevel = null;
			return null;
		}

		public float? BatteryLevel
		{
			get
			{
				if (cachedBatteryLevel != null && DateTimeOffset.Now - whenBatteryLevelCached < TimeSpan.FromSeconds(30))
				{
					return cachedBatteryLevel;
				}
				return null;
			}
		}

		public async Task<string?> GetFirmwareVersionAsync()
		{
			return await PrintShortLuaResponse("frame.FIRMWARE_VERSION", TimeSpan.FromSeconds(1));
		}
		public string? FirmwareVersion
		{
			get
			{
				return this.GetFirmwareVersionAsync().Result;
			}
		}

		public async Task Sleep(TimeSpan? duration = null)
		{
			if (duration == null)
			{
				await SendLua("frame.sleep()");
			}
			else
			{
				await SendLua($"frame.sleep({duration.Value.TotalSeconds})");
			}
		}

		private bool _stayAwake;

		public bool StayAwake
		{
			get { return _stayAwake; }
			set
			{
				if (value != _stayAwake)
				{
					_stayAwake = value;
					if (_stayAwake)
					{
						Helpers.AutoRetry(() => SendLua("frame.stay_awake(true)"));
					}
					else
					{
						Helpers.AutoRetry(() => SendLua("frame.stay_awake(false)"));
					}
				}
			}
		}

		public async Task Display_Clear()
		{
			await SendLua("frame.display.clear()");
		}

		public async Task Display_Show()
		{
			await SendLua("frame.display.show()");
		}

		public async Task Display_Text(string text, int x, int y)
		{
			await SendLua($"frame.display.text(\"{text}\")");
		}

		private readonly ReadOnlyDictionary<char, string> luaEscapeSequences = new ReadOnlyDictionary<char, string>(new Dictionary<char, string>
		{
			{ '\\', @"\\" },
			{ '\n', @"\n" },
			{ '\r', @"\r" },
			{ '\t', @"\t" },
			{ '\"', "\\\"" },
			{ '[', @"\[" },
			{ ']', @"\]" }
		});

		public async Task SaveFile(string filename, string contents)
		{
			contents = contents.Replace(Environment.NewLine, "\n");
			foreach (var kvp in luaEscapeSequences)
			{
				contents = contents.Replace(kvp.Key.ToString(), kvp.Value);
			}
			string varName = "f" + filename.Split('.').First();
			await SendLuaAutoRetry($"{varName} = frame.file.open(\"{filename}\", \"write\")");
			int curIndex = 0;
			int chunkIndex = 1;
			while (curIndex < contents.Length)
			{
				int nextChunkLength = Math.Min(contents.Length - curIndex, (this.Connection.MTUSize ?? 23) - 30 - varName.Length);
				if (nextChunkLength <= 0)
				{
					break;
				}
				while (contents[curIndex + nextChunkLength - 1] == '\\' && nextChunkLength > 0)
				{
					nextChunkLength--;
				}
				if (nextChunkLength <= 0)
				{
					break;
				}
				string partToSend = contents.Substring(curIndex, nextChunkLength);

				await SendLuaAutoRetry($"{varName}:write(\"{partToSend}\")");
				curIndex += nextChunkLength;
				Debug.WriteLine($"chunk {chunkIndex++} written");
			}
			await SendLuaAutoRetry($"{varName}:close()");
			Debug.WriteLine("file written, total of " + chunkIndex + " chunks");
			/*await Connection.SendLua($"fr = frame.file.open(\"{filename}\", \"r\")");
			string readContents = await Connection.PrintLuaResponse("fr:read \"*a\"");
			await Connection.SendLua("fr:close()");
			Debug.WriteLine("expected: " + contents);
			Debug.WriteLine("  actual: " + readContents);
			Debug.Assert(contents == readContents);
			*/
		}

		private string getUnusedScriptName()
		{
			// the id should be 2 chars long
			string id = null;
			do
			{
				id = Convert.ToChar((int)'a' + Random.Shared.Next(26)).ToString() + Convert.ToChar((int)'a' + Random.Shared.Next(26)).ToString();
			} while (scriptNames.Contains(id));
			return id;
		}

		private List<string> scriptNames = new List<string>();

		public async Task RunScriptViaFile(string script, string overrideName = null)
		{
			string name = (overrideName ?? getUnusedScriptName());
			await SaveFile(name + ".lua", script.Replace("\t", ""));
			await SendLuaChecked($"require(\"{name}\")");
		}

		public async Task<bool> RunScriptViaString(string script, string overrideName = null)
		{
			string varName = (overrideName ?? "s"+getUnusedScriptName());

			string contents = script.Replace(Environment.NewLine, "\n");
			foreach (var kvp in luaEscapeSequences)
			{
				script = script.Replace(kvp.Key.ToString(), kvp.Value);
			}
			await SendLuaChecked($"{varName} = ''");
			int curIndex = 0;
			int chunkIndex = 1;
			while (curIndex < contents.Length)
			{
				int nextChunkLength = Math.Min(contents.Length - curIndex, (this.Connection.MTUSize ?? 23) - 30 - varName.Length);
				if (nextChunkLength <= 0)
				{
					break;
				}
				while (contents[curIndex + nextChunkLength - 1] == '\\' && nextChunkLength > 0)
				{
					nextChunkLength--;
				}
				if (nextChunkLength <= 0)
				{
					break;
				}
				string partToSend = contents.Substring(curIndex, nextChunkLength);

				await SendLuaChecked($"{varName} = {varName} . \"{partToSend}\"");
				curIndex += nextChunkLength;
				Debug.WriteLine($"chunk {chunkIndex++} sent");
			}
			
			return await SendLuaChecked($"loadstring({varName})()");
		}

		public int GetByteLengthOfString(string str)
		{
			var writer = new DataWriter();
			writer.WriteString(str);
			return (int)writer.UnstoredBufferLength;
		}

		public async Task<bool> SendLuaAutoRetry(string luaCodeToSend, int? maxTries = 5, TimeSpan? maxTimeout = null)
		{
			return await Helpers.AutoRetry(() => SendLuaChecked(luaCodeToSend), maxTries, maxTimeout);
		}

		public async Task<bool> SendLuaChecked(string luaCodeToSend, TimeSpan? maxTimespanToWaitForReply = null)
		{
			bool confirmed = false;
			// wait for the reply
			string callbackId = registerCallback((receivedString) =>
			{
				confirmed = true;
			});
			if (!await SendLua(luaCodeToSend + $";print(\"~{callbackId}.\")"))
				return false;
			// wait for result to have a value, or for maxTimespanToWaitForReply to elapse
			var waitForReplyTask = Task.Run(() =>
			{
				while (!confirmed)
				{
					Task.Delay(20).Wait();
				}
			});
			if (maxTimespanToWaitForReply.HasValue)
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(maxTimespanToWaitForReply.Value));
			}
			else
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(TimeSpan.FromSeconds(10)));
			}
			return confirmed;
		}

		public async Task<bool> SendLua(string luaCodeToSend)
		{
			if (!await Connection.Connect())
			{
				Debug.WriteLine("Failed to connect");

				await Task.Delay(1000);
				if (!await Connection.Connect())
				{
					Debug.WriteLine("Failed to connect again");
					return false;
					throw new AccessViolationException("Failed to connect to Frame");
				}
			}

			if (GetByteLengthOfString(luaCodeToSend) > Connection.MTUSize)
			{
				if (Connection.MTUSize < 50)
				{
					Debug.WriteLine("MTU too small to send in one go");
					// wait 500 ms and try again
					await Task.Delay(500);
					if (GetByteLengthOfString(luaCodeToSend) > Connection.MTUSize - 3)
					{
						Debug.WriteLine("MTU still too small to send in one go");
						return false;
						throw new ArgumentOutOfRangeException("The string is too long to send in one go");
					}
				}
				else
				{
					Debug.WriteLine("String too long to send in one go");
					return false;
					throw new ArgumentOutOfRangeException("The string is too long to send in one go");
				}
			}

			Debug.WriteLine($"Sending: {luaCodeToSend}");
			Debug.WriteLine($"Length: {luaCodeToSend.Length}, MTU: {Connection.MTUSize - 3}");

			if (Connection.sendCharacteristic != null)
			{
				using var writer = new DataWriter();
				writer.WriteString(luaCodeToSend);
				var buffer = writer.DetachBuffer();
				return await SendRaw(buffer);
			}

			Debug.WriteLine("sendCharacteristic is null");
			Task.Delay(1000).Wait();
			if (await Connection.Connect())
			{
				return await SendLua(luaCodeToSend);
			}
			else
			{
				return false;
			}
		}

		public async Task<bool> SendRaw(IBuffer data)
		{
			if (Connection.sendCharacteristic != null)
			{
				try
				{
					await Connection.sendCharacteristic.WriteValueAsync(data, GattWriteOption.WriteWithoutResponse);
					return true;
				}
				catch (ArgumentException)
				{
					Debug.WriteLine($"Failed to send data: ArgumentException.   Data length: {data.Length.ToString()}, MTU: {Connection.MTUSize}.");
					return false;
				}
				catch (OperationCanceledException)
				{
					Debug.WriteLine($"Failed to send data: OperationCanceledException.  IsConnected: {Connection.IsConnected}");
					return false;
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
					return false;
				}

			}
			return false;
		}

		public async Task<bool> SendRaw(byte data)
		{
			using var writer = new DataWriter();
			writer.WriteByte(data);
			var buffer = writer.DetachBuffer();
			return await SendRaw(buffer);
		}


		public async Task<string> SendLuaWithResponse(string luaCodeToSend, TimeSpan? maxTimespanToWaitForReply = null)
		{
			await SendLua(luaCodeToSend);
			string result = null;
			// wait for the reply
			_onReceive = (receivedString) =>
			{
				result = receivedString;
			};
			// wait for result to have a value, or for maxTimespanToWaitForReply to elapse
			var waitForReplyTask = Task.Run(() =>
			{
				while (result == null)
				{
					Thread.Yield();
					Thread.Sleep(50);
				}
			});
			if (maxTimespanToWaitForReply.HasValue)
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(maxTimespanToWaitForReply.Value));
			}
			else
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(TimeSpan.FromSeconds(30)));
			}
			return result;
		}

		public async Task<string> PrintShortLuaResponse(string luaExpression, TimeSpan? maxTimespanToWaitForReply = null)
		{
			string result = null;
			// wait for the reply
			string callbackId = registerCallback((receivedString) =>
			{
				result = receivedString;
			});
			await SendLua($"print(\"~{callbackId}:\"..{luaExpression})");
			// wait for result to have a value, or for maxTimespanToWaitForReply to elapse
			var waitForReplyTask = Task.Run(() =>
			{
				while (result == null)
				{
					Thread.Yield();
					Thread.Sleep(50);
				}
			});
			if (maxTimespanToWaitForReply.HasValue)
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(maxTimespanToWaitForReply.Value));
			}
			else
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(TimeSpan.FromSeconds(10)));
			}
			return result;
		}

		public async Task<string> PrintLuaResponse(string luaExpression, TimeSpan? maxTimespanToWaitForReply = null)
		{
			if (!printLongInjected)
			{
				await initLongPrint();
			}
			string result = null;
			// wait for the reply
			string callbackId = registerCallback((receivedString) =>
			{
				result = receivedString;
			});
			await SendLua($"prntLng(\"{callbackId}\",{luaExpression})");
			// wait for result to have a value, or for maxTimespanToWaitForReply to elapse
			var waitForReplyTask = Task.Run(() =>
			{
				while (result == null)
				{
					Thread.Yield();
					Thread.Sleep(50);
				}
			});
			if (maxTimespanToWaitForReply.HasValue)
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(maxTimespanToWaitForReply.Value));
			}
			else
			{
				await Task.WhenAny(waitForReplyTask, Task.Delay(TimeSpan.FromSeconds(10)));
			}
			return result;
		}


		private bool printLongInjected = false;
		private async Task initLongPrint()
		{
			string exisits = await PrintShortLuaResponse("tostring(prntLng ~= nil)");
			if (exisits != "true")
			{
				printLongInjected = false;
				Debug.Print("prntLng does not exist, creating it");
				await RunScriptViaFile(returnHelperScript, "prlng");
				Debug.Print("prntLng created");
				printLongInjected = true;
			}
			printLongInjected = true;
		}


		private string registerCallback(Action<string> callback)
		{
			string callbackId = getUnusedCallbackId();
			callbacks.Add(callbackId, callback);
			return callbackId;
		}
		private string getUnusedCallbackId()
		{
			// the id should be 3 chars long
			string id = null;
			do
			{
				id = Guid.NewGuid().ToString().Substring(0, 3);
			} while (callbacks.ContainsKey(id));
			return id;
		}
		private Dictionary<string, Action<string>> callbacks = new Dictionary<string, Action<string>>();
		private Dictionary<string, List<string>> payloads = new Dictionary<string, List<string>>();
		private Action<string> _onReceive;

		private const string returnHelperScript = """
			local mtu = frame.bluetooth.max_length()
			frame.print('mtu: '..mtu)
			function prntLng(id,stringToPrint)
				local len = string.len(stringToPrint)
				local i = 1
				local chunkIndex = 0
				while i <= len do
					local chunk = string.sub(stringToPrint, i, i+mtu-11)
					frame.print('~'..id..'-'..chunkIndex..':'..chunk)
					chunkIndex = chunkIndex + 1
					i = i + mtu
				end
				frame.print('cb~'..id..'!'..chunkIndex)
			end
			frame.print('prntLng injected')
			frame.display.text('Hello world', 50, 100)
			frame.display.show()
			""";

		public async Task<bool> Stop()
		{
			return await SendRaw(3);
		}
		public async Task<bool> Reset()
		{
			return await Helpers.AutoRetry(() => SendRaw(4), 3);
		}
	}
}
