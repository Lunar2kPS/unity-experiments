using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UDPNetworking {
    public class UDPTester : MonoBehaviour {
        [Serializable]
        public struct SerializableIPEndpoint {
            public string address;
            public ushort port;

            public IPEndPoint ToEndpoint() {
                return new IPEndPoint(
                    IPAddress.Parse(address),
                    port
                );
            }
        }

        [Serializable]
        public struct MessageData {
            public string dateTime;
            public string text;

            public MessageData(string text) {
                dateTime = DateTime.Now.Ticks.ToString();
                this.text = text;
            }

            public static MessageData FromBytes(byte[] data) {
                string json = Encoding.UTF8.GetString(data);
                return JsonUtility.FromJson<MessageData>(json);
            }

            public byte[] ToBytes() {
                string json = JsonUtility.ToJson(this);
                return Encoding.UTF8.GetBytes(json);
            }
        }

        [SerializeField] private SerializableIPEndpoint receiverEndpoint;
        [SerializeField] private SerializableIPEndpoint senderEndpoint;

        private UdpClient client;
        private IPEndPoint receiverEndpointInternal;
        private IPEndPoint senderEndpointInternal;

        private CancellationTokenSource cancellation;
        private CancellationToken cancelToken;
        private Task receiveTask;

        private Keyboard keyboard;

        private void OnEnable() {
            receiverEndpointInternal = receiverEndpoint.ToEndpoint();
            senderEndpointInternal = senderEndpoint.ToEndpoint();
            client = new UdpClient(receiverEndpointInternal);

            keyboard = InputSystem.GetDevice<Keyboard>();
            cancellation = new();
            cancelToken = cancellation.Token;
            receiveTask = Task.Run(ReceiveLoop);
        }

        private void OnDisable() {
            cancellation.Cancel();
            try {
                receiveTask.Wait(1000);
            } catch { }

            if (client != null) {
                client.Dispose();
                client = null;
            }
        }

        private void Update() {
            if (keyboard != null) {
                if (keyboard[Key.Space].wasPressedThisFrame) {
                    byte[] bytes = new MessageData("Spacebar was pressed!").ToBytes();
                    client.Send(bytes, bytes.Length, senderEndpointInternal);
                }
            }
        }

        private async Task ReceiveLoop() {
            try {
                Task<UdpReceiveResult> cancelTask = Task.Run(async () => {
                    await cancelToken;
                    return default(UdpReceiveResult);
                });
                while (true) {
                    cancelToken.ThrowIfCancellationRequested();

                    Task<UdpReceiveResult> receiveTask = client.ReceiveAsync();
                    Task<Task<UdpReceiveResult>> current = Task.WhenAny(receiveTask, cancelTask);

                    await current;
                    Task<UdpReceiveResult> completedTask = current.Result;
                    if (completedTask == receiveTask) {
                        byte[] data = receiveTask.Result.Buffer;
                        MessageData message = MessageData.FromBytes(data);
                        Debug.Log("RECEIVED! " + data.Length + " B:\n" + message.dateTime + " -- " + message.text);
                    }
                }
            } catch (OperationCanceledException e) {
                Debug.Log("CANCELLED by " + e.GetType().Name + "!");
                throw;
            } catch (Exception e) {
                Debug.LogException(e);
                throw;
            }
        }
    }
}
