using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

namespace FrameHelper
{
	public class FrameBluetoothLEConnection
	{
		private const string FrameServiceUUID = "7A230001-5475-A6A4-654C-8431F6AD49C4";
		private const string FrameSendCharacteristicUUID = "7A230002-5475-A6A4-654C-8431F6AD49C4";
		private const string FrameReceiveCharacteristicUUID = "7A230003-5475-A6A4-654C-8431F6AD49C4";

		internal GattCharacteristic sendCharacteristic;
		internal GattCharacteristic receiveCharacteristic;

		public bool IsPaired { get; private set; } = false;


		public int? MTUSize { get; private set; } = null;

		private DeviceInformation deviceInfo;

		public FrameBluetoothLEConnection()
		{

		}

		private static string CharacteristicPropertiesToString(GattCharacteristicProperties properties)
		{
			List<string> result = new List<string>();
			if ((properties & GattCharacteristicProperties.Broadcast) == GattCharacteristicProperties.Broadcast)
			{
				result.Add("Broadcast");
			}
			if ((properties & GattCharacteristicProperties.Read) == GattCharacteristicProperties.Read)
			{
				result.Add("Read");
			}
			if ((properties & GattCharacteristicProperties.WriteWithoutResponse) == GattCharacteristicProperties.WriteWithoutResponse)
			{
				result.Add("WriteWithoutResponse");
			}
			if ((properties & GattCharacteristicProperties.Write) == GattCharacteristicProperties.Write)
			{
				result.Add("Write");
			}
			if ((properties & GattCharacteristicProperties.Notify) == GattCharacteristicProperties.Notify)
			{
				result.Add("Notify");
			}
			if ((properties & GattCharacteristicProperties.Indicate) == GattCharacteristicProperties.Indicate)
			{
				result.Add("Indicate");
			}
			if ((properties & GattCharacteristicProperties.AuthenticatedSignedWrites) == GattCharacteristicProperties.AuthenticatedSignedWrites)
			{
				result.Add("AuthenticatedSignedWrites");
			}
			if ((properties & GattCharacteristicProperties.ExtendedProperties) == GattCharacteristicProperties.ExtendedProperties)
			{
				result.Add("ExtendedProperties");
			}
			if ((properties & GattCharacteristicProperties.ReliableWrites) == GattCharacteristicProperties.ReliableWrites)
			{
				result.Add("ReliableWrites");
			}
			if ((properties & GattCharacteristicProperties.WritableAuxiliaries) == GattCharacteristicProperties.WritableAuxiliaries)
			{
				result.Add("WritableAuxiliaries");
			}
			return string.Join(", ", result.ToArray());
		}

		BluetoothLEAdvertisementWatcher watcher = null;

		ulong? deviceAddress = null;
		GattSession currentSession = null;

		public bool IsConnected
		{
			get
			{
				if (!deviceAddress.HasValue)
				{
					return false;
				}
				if (sendCharacteristic == null || receiveCharacteristic == null)
				{
					return false;
				}
				return currentSession.SessionStatus == GattSessionStatus.Active;
			}
		}

		public async Task<bool> Connect()
		{
			if (IsConnected)
				return true;

			if (deviceAddress.HasValue)
			{
				var device = await BluetoothLEDevice.FromBluetoothAddressAsync(deviceAddress.Value);
				if (device != null)
				{
					var deviceId = device.BluetoothDeviceId;
					if (deviceId == null)
					{
						return false;
					}
					currentSession = await GattSession.FromDeviceIdAsync(deviceId);
					if (currentSession == null)
					{
						return false;
					}
					//currentSession.MaintainConnection = true;
					this.MTUSize = (int)currentSession.MaxPduSize;
					currentSession.SessionStatusChanged += CurrentSession_SessionStatusChanged;
					currentSession.MaxPduSizeChanged += (sender, args) =>
					{
						if (sender == this.currentSession)
						{
							Debug.WriteLine($"MTU changed to {sender.MaxPduSize}");
							MTUSize = (int)sender.MaxPduSize;
						}
					};

					var service = await device.GetGattServicesForUuidAsync(new Guid(FrameServiceUUID), BluetoothCacheMode.Cached);
					if (service.Status == GattCommunicationStatus.Success)
					{
						Debug.WriteLine("Service found");

						var characteristics = await service.Services[0].GetCharacteristicsForUuidAsync(new Guid(FrameSendCharacteristicUUID), BluetoothCacheMode.Cached);
						if (characteristics.Status == GattCommunicationStatus.Success)
						{
							sendCharacteristic = characteristics.Characteristics[0];
							Debug.WriteLine($"Send characteristic found: {CharacteristicPropertiesToString(sendCharacteristic.CharacteristicProperties)}");
						}
						characteristics = await service.Services[0].GetCharacteristicsForUuidAsync(new Guid(FrameReceiveCharacteristicUUID), BluetoothCacheMode.Cached);
						if (characteristics.Status == GattCommunicationStatus.Success)
						{
							receiveCharacteristic = characteristics.Characteristics[0];
							receiveCharacteristic.ValueChanged += ReceiveCharacteristic_ValueChanged;
							Debug.WriteLine($"Receive characteristic found: {CharacteristicPropertiesToString(receiveCharacteristic.CharacteristicProperties)}");

							var curDescriptors = await receiveCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
							if (true) //curDescriptors.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
							{
								Debug.WriteLine("Not yet subscribed to the notification");

								GattCommunicationStatus notifyStatus = await receiveCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
									GattClientCharacteristicConfigurationDescriptorValue.Notify);
								if (notifyStatus == GattCommunicationStatus.Success)
								{
									Debug.WriteLine("Subscribing to the Notification");
								}
								else
								{
									Debug.WriteLine("Failed to subscribe to the Notification: " + notifyStatus.ToString());
									receiveCharacteristic = null;
								}
							}
							else
							{
								Debug.WriteLine("Already subscribed to the Notification");
							}
						}
						
						return IsConnected;
					}
				}
			}
			else
			{
				if (!IsSearching)
				{
					IsSearching = true;
				}

				// wait for the watcher to find the device
				var waitForDeviceTask = Task.Run(() =>
				{
					while (!deviceAddress.HasValue)
					{
						Task.Delay(50).Wait();
					}
				});
				await Task.WhenAny(waitForDeviceTask, Task.Delay(TimeSpan.FromSeconds(10)));
				if (deviceAddress.HasValue)
				{
					return await Connect();
				}
			}
			return false;
		}

		private void CurrentSession_SessionStatusChanged(GattSession sender, GattSessionStatusChangedEventArgs args)
		{
			if (sender == currentSession)
			{
				Debug.WriteLine($"Session status changed: {args.Status}");
				if (args.Status == GattSessionStatus.Closed)
				{
					/*
					currentSession = null;
					sendCharacteristic = null;
					receiveCharacteristic = null;
					*/
				}
			}
		}

		public bool IsSearching
		{
			get
			{
				if (watcher != null)
				{
					return watcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;
				}
				else
				{
					return false;
				}
			}
			set
			{
				if (value)
				{
					if (watcher == null)
					{
						watcher = new BluetoothLEAdvertisementWatcher();
						watcher.ScanningMode = BluetoothLEScanningMode.Active;
						watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(new Guid(FrameServiceUUID));
						watcher.Received += Watcher_Received;
						watcher.Start();
					}
					else
					{
						if (watcher.Status == BluetoothLEAdvertisementWatcherStatus.Stopped)
						{
							watcher.Start();
						}
					}
				}
				else
				{
					if (watcher != null)
					{
						watcher.Stop();
					}
				}
			}
		}
		private async void Watcher_Received(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
		{
			deviceAddress = args.BluetoothAddress;
			var device = await BluetoothLEDevice.FromBluetoothAddressAsync(args.BluetoothAddress);
			Debug.WriteLine($"BLEWATCHER Found: {device.DeviceInformation.Name}");

			device.ConnectionStatusChanged += Device_ConnectionStatusChanged;

			IsSearching = false;

			if (!device.DeviceInformation.Pairing.IsPaired && device.DeviceInformation.Pairing.CanPair)
			{
				var result = await device.DeviceInformation.Pairing.PairAsync(DevicePairingProtectionLevel.Encryption);
				Debug.WriteLine($"Pairing result: {result.Status}");

				if (result.Status == DevicePairingResultStatus.Paired)
				{
					IsPaired = true;
					OnStatusChanged?.Invoke(this, IsConnected, IsPaired, !IsSearching);

					Debug.WriteLine("Paired successfully");
				}
				else
				{
					IsPaired = false;
					OnStatusChanged?.Invoke(this, IsConnected, IsPaired, !IsSearching);

					Debug.WriteLine("Pairing failed");
					return;
				}
			}

			IsPaired = device.DeviceInformation.Pairing.IsPaired;

			if (IsPaired)
			{

				Debug.WriteLine("Device is Paired");
			}

			if (IsConnected)
			{
				Debug.WriteLine("Already connected");
			}
			else
			{
				Debug.WriteLine("Not connected");
			}

			OnStatusChanged?.Invoke(this, IsConnected, IsPaired, !IsSearching);

		}

		private void Device_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
		{
			Debug.WriteLine($"Connection status changed: {sender.ConnectionStatus}");
			OnStatusChanged?.Invoke(this, IsConnected, IsPaired, !IsSearching);
		}

		// a public event for dataReceived
		public delegate void ReceiveString(FrameBluetoothLEConnection sender, string receivedString);
		public event ReceiveString OnReceiveString;
		public delegate void ReceiveBytes(FrameBluetoothLEConnection sender, byte streamId, Stream stream);
		public event ReceiveBytes OnReceivedBytes;
		public delegate void StatusChanged(FrameBluetoothLEConnection sender, bool isConnected, bool isPaired, bool isFound);
		public event StatusChanged OnStatusChanged;

		private Dictionary<byte, MemoryStream> DataStreams = new Dictionary<byte, MemoryStream>();

		private void ReceiveCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
		{
			Debug.WriteLine("Received value changed event");
			// get the value.
			byte firstByte = args.CharacteristicValue.GetByte(0);
			if (firstByte == 1)
			{
				Debug.WriteLine("Receiving data");
				byte streamIdSigned = args.CharacteristicValue.GetByte(1);
				byte streamId = (byte)(streamIdSigned & 0b01111111);
				if (streamIdSigned >= 128)
				{
					// ending the stream
					if (DataStreams.ContainsKey(streamId))
					{
						DataStreams[streamId].Seek(0, System.IO.SeekOrigin.Begin);
						OnReceivedBytes?.Invoke(this, streamId, DataStreams[streamId]);
						//DataStreams[streamId].Dispose();
						DataStreams.Remove(streamId);
						Debug.WriteLine($"Stream {streamId} ended");
					}
					else if (args.CharacteristicValue.Length > 2)
					{
						// sending data that is only one MTU long
						// using?
						var stream = new MemoryStream(args.CharacteristicValue.ToArray(), 2, (int)(args.CharacteristicValue.Length) - 2);
						stream.Seek(0, System.IO.SeekOrigin.Begin);
						OnReceivedBytes?.Invoke(this, streamId, stream);
						Debug.WriteLine($"Stream {streamId} started and ended");
					} else { 
						// trying to end a stream that doesn't exist
						Debug.WriteLine($"Stream {streamId} not found, can't end");
					}
				}
				else
				{
					// sending stream data
					if (!DataStreams.ContainsKey(streamId))
					{
						DataStreams.Add(streamId, new MemoryStream());
					}
					DataStreams[streamId].Write(args.CharacteristicValue.ToArray(), 2, (int)(args.CharacteristicValue.Length) - 2);
				}
				return;
			}
			using var reader = DataReader.FromBuffer(args.CharacteristicValue);
			// peek at the first byte.  If it is not a 1, then put it back
			

			var receivedString = reader.ReadString(reader.UnconsumedBufferLength);
			if (receivedString != null)
			{
				Debug.WriteLine($"Received: {receivedString}");
				OnReceiveString?.Invoke(this, receivedString);
			}
		}


		public async Task<bool> IsFrameInRange()
		{
			if (deviceInfo != null && sendCharacteristic != null && receiveCharacteristic != null)
			{
				// check if the deviceInfo device is in range

				var device = await BluetoothLEDevice.FromIdAsync(deviceInfo.Id);
				if (device != null)
				{
					return device.ConnectionStatus == BluetoothConnectionStatus.Connected;

				}
			}

			return false;
		}


	}
}
