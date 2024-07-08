using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

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
			Connection.OnReceiveString += Connection_OnReceiveData;
			Connection.OnStatusChanged += Connection_OnStatusChanged;
			Connection.OnReceivedBytes += Connection_OnReceivedBytes;
		}

		private void Connection_OnReceivedBytes(FrameBluetoothLEConnection sender, byte streamId, Stream stream)
		{
			Debug.WriteLine($"Received start of stream {streamId}");
			if (streamId < 20)
			{
				string textData = null;
				using (var reader = new StreamReader(stream))
				{
					textData = reader.ReadToEnd();
				}
			}
			if (streamId >= 20 && streamId < 30)
			{
				Bitmap img = Bitmap.FromStream(stream) as Bitmap;
				if (imageCallbacks.ContainsKey(streamId))
				{
					imageCallbacks[streamId](img);
					imageCallbacks.Remove(streamId);
				}
			}
		}

		private void Connection_OnStatusChanged(FrameBluetoothLEConnection sender, bool isConnected, bool isPaired, bool isFound)
		{
			if (isConnected && Connection.MTUSize > 100 && !hasInitedHelpers)
			{
				Task.Run(() =>
				{
					lock (Connection)
					{
						if (!hasInitedHelpers)
							hasInitedHelpers = this.InjectAllLibraryFunctions().Result;
					}
				});
			}
		}

		private bool hasInitedHelpers = false;

		private void Connection_OnReceiveData(FrameBluetoothLEConnection sender, string receivedString)
		{
			if (receivedString.StartsWith("~"))
			{
				if (receivedString[4] == ':')
				{
					string callbackId = receivedString.Substring(1, 3);
					string payload = receivedString.Substring(5);
					if (textCallbacks.ContainsKey(callbackId))
					{
						textCallbacks[callbackId](payload);
						textCallbacks.Remove(callbackId);
					}
					else
					{
						Debug.WriteLine($"Callback {callbackId} not found");
					}
				}
				else if (receivedString[4] == '.')
				{
					string callbackId = receivedString.Substring(1, 3);
					if (textCallbacks.ContainsKey(callbackId))
					{
						textCallbacks[callbackId](callbackId);
						textCallbacks.Remove(callbackId);
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

					if (!replyPayloads.ContainsKey(callbackId))
					{
						replyPayloads.Add(callbackId, new List<string>());
					}
					if (replyPayloads[callbackId].Count == callbackIndex)
					{
						replyPayloads[callbackId].Add(payload);
					}
					else if (callbackIndex < replyPayloads[callbackId].Count)
					{
						replyPayloads[callbackId][callbackIndex] = payload;
					}
					else if (callbackIndex > replyPayloads[callbackId].Count)
					{
						for (int i = replyPayloads[callbackId].Count; i < callbackIndex; i++)
						{
							replyPayloads[callbackId].Add(null);
						}
					}
				}
				else if (receivedString[4] == '!')
				{
					string callbackId = receivedString.Substring(1, 3);
					int totalChunks = int.Parse(receivedString.Substring(5));
					if (!replyPayloads.ContainsKey(callbackId))
					{
						replyPayloads.Add(callbackId, new List<string>());
					}
					if (replyPayloads[callbackId].Count == totalChunks)
					{
						string payload = string.Join("", replyPayloads[callbackId]);
						if (textCallbacks.ContainsKey(callbackId))
						{
							textCallbacks[callbackId](payload);
							textCallbacks.Remove(callbackId);
						}
						else
						{
							Debug.WriteLine($"Callback for {callbackId} not found");
						}
						replyPayloads.Remove(callbackId);
					}
					else
					{
						Debug.WriteLine($"Expected {totalChunks} chunks, but got {replyPayloads[callbackId].Count}");
					}

				}
			}
			else if (_onReceive != null)
			{
				_onReceive(receivedString);
			}

			if (Connection.IsConnected && Connection.MTUSize > 100 && !hasInitedHelpers)
			{
				Task.Run(() =>
				{
					lock (Connection)
					{
						if (!hasInitedHelpers)
							hasInitedHelpers = this.InjectAllLibraryFunctions().Result;
					}
				});
			}
		}

		public Frame(FrameBluetoothLEConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this.Connection = connection;
			this.Connection.OnReceiveString += Connection_OnReceiveData;
		}

		float? cachedBatteryLevel = null;
		DateTimeOffset whenBatteryLevelCached = DateTimeOffset.MinValue;
		public async Task<float?> GetBatteryLevelAsync()
		{
			if (DateTimeOffset.Now - whenBatteryLevelCached < TimeSpan.FromSeconds(30))
			{
				return cachedBatteryLevel;
			}

			string result = null;

			if (Connection.MTUSize > 100)
				result = await PrintShortLuaResponse("frame.battery_level()", TimeSpan.FromSeconds(5));
			else
			{
				cachedBatteryLevel = null;
				whenBatteryLevelCached = DateTimeOffset.Now;
				return null;
			}
			if (result != null && float.TryParse(result, out float batteryLevel))
			{
				cachedBatteryLevel = batteryLevel;
				whenBatteryLevelCached = DateTimeOffset.Now;
				return batteryLevel;
			}
			cachedBatteryLevel = null;
			whenBatteryLevelCached = DateTimeOffset.Now;
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



		public async Task SaveFile(string filename, string contents)
		{
			await FrameFile.WriteFile(this, filename, contents);
		}

		private string getUnusedScriptName()
		{
			// the id should be 4 chars long
			string id = null;
			do
			{
				id = "";
				for (int i = 0; i < 4; i++)
				{
					id += Convert.ToChar((int)'a' + Random.Shared.Next(26)).ToString();
				}
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

		public int GetByteLengthOfString(string str)
		{
			var writer = new Windows.Storage.Streams.DataWriter();
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
					Debug.WriteLine($"String too long to send in one go ({GetByteLengthOfString(luaCodeToSend)} / {Connection.MTUSize})");
					Debug.WriteLine(luaCodeToSend);
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

		public async Task<string> PrintShortLuaResponse(string luaExpression, TimeSpan? maxTimespanToWaitForReply = null, string runBeforePrinting = null)
		{
			string result = null;
			// wait for the reply
			string callbackId = registerCallback((receivedString) =>
			{
				result = receivedString;
			});
			if (runBeforePrinting != null)
				await SendLua(runBeforePrinting + $";print(\"~{callbackId}:\"..{luaExpression})");
			else
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

		public async Task<string> PrintLuaResponse(string luaExpression, TimeSpan? maxTimespanToWaitForReply = null, string runBeforePrinting = null)
		{
			string result = null;
			// wait for the reply
			string callbackId = registerCallback((receivedString) =>
			{
				result = receivedString;
			});
			if (runBeforePrinting != null)
				await SendLua(runBeforePrinting + $";prntLng(\"{callbackId}\",{luaExpression})");
			else
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

		private string registerCallback(Action<string> callback)
		{
			string callbackId = getUnusedCallbackId();
			textCallbacks.Add(callbackId, callback);
			return callbackId;
		}
		private string getUnusedCallbackId()
		{
			// the id should be 3 chars long
			string id = null;
			do
			{
				id = Guid.NewGuid().ToString().Substring(0, 3);
			} while (textCallbacks.ContainsKey(id));
			return id;
		}

		private Dictionary<byte, Action<Vector3>> motionCallbacks = new Dictionary<byte, Action<Vector3>>();
		private Dictionary<byte, Action<Memory<double>>> audioCallbacks = new Dictionary<byte, Action<Memory<double>>>();
		private Dictionary<byte, Action<Bitmap>> imageCallbacks = new Dictionary<byte, Action<Bitmap>>();
		private Dictionary<string, Action<string>> textCallbacks = new Dictionary<string, Action<string>>();
		private Dictionary<string, List<string>> replyPayloads = new Dictionary<string, List<string>>();
		private Action<string> _onReceive;

		public async Task<bool> Stop()
		{
			Debug.WriteLine("Stopping");
			bool succeeded = await SendRaw(3);
			Debug.WriteLine("Stop " + (succeeded ? "succeeded" : "failed"));
			return succeeded;
		}
		public async Task<bool> Reset()
		{
			Debug.WriteLine("Resetting");
			bool succeeded = await SendRaw(4);
			Debug.WriteLine("Reset " + (succeeded ? "succeeded" : "failed"));

			if (succeeded)
			{
				this.hasInitedHelpers = false;
			}

			return succeeded;
		}

		public FrameDirectory GetFilesystemRoot()
		{
			return new FrameDirectory(this, "/");
		}

		public async Task<Bitmap> GetCameraPhoto(int quality = 50, bool autoExposure = true)
		{
			switch (quality)
			{
				case 1:
					quality = 10;
					break;
				case 2:
					quality = 25;
					break;
				case 3:
					quality = 50;
					break;
				case 4:
					quality = 100;
					break;
				case 10:
					break;
				case 25:
					break;
				case 50:
					break;
				case 100:
					break;
				default:
					throw new ArgumentOutOfRangeException("quality", "Quality must be 10, 25, 50, or 100");
			}
			await SendLuaChecked("mtu = frame.bluetooth.max_length();frame.camera.wake()");

			if (autoExposure)
			{
				await SendLuaChecked("for _=1, 20 do;frame.camera.auto{};frame.sleep(0.1);end");
			}

			byte thisCalbackIndex = 255;

			for (int i = 20; i < 30; i++)
			{
				if (!imageCallbacks.ContainsKey((byte)i))
				{
					thisCalbackIndex = (byte)i;
					break;
				}
			}

			if (thisCalbackIndex == 255)
			{
				throw new InvalidOperationException("No available callback index");
			}

			TaskCompletionSource<Bitmap> tcs = new TaskCompletionSource<Bitmap>();
			imageCallbacks.Add(thisCalbackIndex, (img) =>
			{
				tcs.SetResult(img);
			});

			await SendLuaChecked("frame.camera.capture{quality_factor=" + quality.ToString() + "};");
			await SendLua($"while true do;local i=frame.camera.read(frame.bluetooth.max_length()-1) if (i==nil) then break end while true do if pcall(frame.bluetooth.send,'\\x01'..string.char({thisCalbackIndex})..i) then break end end end;bluetooth.send('\\x01'..string.char({thisCalbackIndex+128}))");

			return await tcs.Task;
		}
	}
}
